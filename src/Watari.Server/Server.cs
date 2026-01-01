
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Watari;

public class Server(IOptions<ServerOptions> options, IServiceProvider serviceProvider, ILogger<Server> logger)
{
    private JsonSerializerOptions? _jsonOptions;
    private readonly HttpListener _listener = new();

    public ServerOptions Options { get; } = options.Value;
    public WebSocket? EventWebSocket { get; set; }
    private JsonSerializerOptions JsonOptions => _jsonOptions ??= BuildSerializerOptions();

    private JsonSerializerOptions BuildSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        foreach (var converter in Options.JsonConverters)
        {
            options.Converters.Add(converter);
        }
        return options;
    }

    public async Task StartAsync()
    {
        _listener.Prefixes.Add($"http://localhost:{Options.ServerPort}/");
        _listener.Start();

        _ = Task.Run(async () =>
        {
            while (_listener.IsListening && !Options.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context));
                }
                catch (Exception ex)
                {
                    if (_listener.IsListening)
                    {
                        logger.LogError(ex, "Error accepting connection");
                    }
                }
            }
        }, Options.CancellationToken);

        await Task.CompletedTask;
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // CORS
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        if (request.HttpMethod == "OPTIONS")
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
            return;
        }

        try
        {
            if (request.Url?.AbsolutePath == "/invoke" && request.HttpMethod == "POST")
            {
                await HandleInvokeAsync(context);
            }
            else if (request.Url?.AbsolutePath == "/events" && request.IsWebSocketRequest)
            {
                await HandleWebSocketAsync(context);
            }
            else if (request.HttpMethod == "GET" && !Options.Dev)
            {
                await ServeStaticFileAsync(context);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Close();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling request {Method} {Url}", request.HttpMethod, request.Url);
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.Close();
        }
    }

    private async Task HandleInvokeAsync(HttpListenerContext context)
    {
        using var reader = new StreamReader(context.Request.InputStream);
        var body = await reader.ReadToEndAsync();
        var invokeRequest = JsonSerializer.Deserialize<InvokeRequest>(body, JsonOptions);

        if (invokeRequest == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
            return;
        }

        logger.LogDebug("Invoking {Method}", invokeRequest.Method);

        var parts = invokeRequest.Method.Split('.');
        if (parts.Length != 2)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
            return;
        }

        var typeName = parts[0];
        var methodName = parts[1];

        var type = Options.ExposedTypes.FirstOrDefault(t => t.Name == typeName);
        if (type == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Close();
            return;
        }

        var actionMethod = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        if (actionMethod == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Close();
            return;
        }

        var parameters = actionMethod.GetParameters();
        if (parameters.Length != invokeRequest.Args.Count)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
            return;
        }

        try
        {
            var args = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                var arg = invokeRequest.Args[i];
                args[i] = arg.Deserialize(paramType, JsonOptions);
            }

            var instance = serviceProvider.GetRequiredService(type);
            var actionValue = actionMethod.Invoke(instance, args);
            var finalValue = await AwaitIfTask(actionValue);

            if (finalValue == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
            else
            {
                var json = JsonSerializer.Serialize(finalValue, JsonOptions);
                var buffer = Encoding.UTF8.GetBytes(json);
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {Method}", invokeRequest.Method);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            context.Response.Close();
        }
    }

    private async Task HandleWebSocketAsync(HttpListenerContext context)
    {
        if (EventWebSocket != null)
        {
            await EventWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }

        var wsContext = await context.AcceptWebSocketAsync(null);
        EventWebSocket = wsContext.WebSocket;

        var buffer = new byte[1024 * 4];
        try
        {
            while (EventWebSocket.State == WebSocketState.Open)
            {
                var result = await EventWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await EventWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "WebSocket connection closed");
        }
        finally
        {
            EventWebSocket = null;
        }
    }

    private async Task ServeStaticFileAsync(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath ?? "/";
        if (path == "/") path = "/index.html";

        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var filePath = Path.Combine(baseDir, "wwwroot", path.TrimStart('/'));

        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(baseDir, path.TrimStart('/'));
        }

        if (File.Exists(filePath))
        {
            var buffer = await File.ReadAllBytesAsync(filePath);
            context.Response.ContentType = GetContentType(filePath);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        context.Response.Close();
    }

    private string GetContentType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".html" => "text/html",
            ".js" => "application/javascript",
            ".css" => "text/css",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }

    private static async Task<object?> AwaitIfTask(object? value)
    {
        if (value is not Task task)
        {
            return value;
        }

        await task;

        var propertyInfo = task.GetType().GetProperty(nameof(Task<object>.Result));
        if (propertyInfo == null)
        {
            return null;
        }
        return propertyInfo.GetValue(task);
    }

    public async Task EmitEvent(string eventName, object data)
    {
        if (EventWebSocket != null && EventWebSocket.State == WebSocketState.Open)
        {
            var msg = JsonSerializer.Serialize(new { @event = eventName, data }, JsonOptions);
            var buffer = Encoding.UTF8.GetBytes(msg);
            await EventWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
