
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Watari;

public class Server(IOptions<ServerOptions> options, IServiceProvider serviceProvider)
{
    private JsonSerializerOptions? _jsonOptions;

    public WebApplication WebApplication { get; private set; } = null!;
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
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        });
        builder.Services.AddCors()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Warning));
        WebApplication webApplication = builder.Build();
        webApplication.Urls.Add($"http://localhost:{Options.ServerPort}");
        webApplication.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        webApplication.UseWebSockets();
        webApplication.MapPost("/invoke", (Delegate)HandleRequest);
        webApplication.MapGet("/events", HandleWebSocket);
        WebApplication = webApplication;

        if (!Options.Dev)
        {
            webApplication.UseStaticFiles();
        }

        await webApplication.StartAsync(Options.CancellationToken);
    }

    private async Task<IResult> HandleRequest(HttpContext context)
    {
        var request = await JsonSerializer.DeserializeAsync<InvokeRequest>(context.Request.Body, JsonOptions);
        if (request == null) return Results.BadRequest();

        var parts = request.Method.Split('.');
        if (parts.Length != 2) return Results.BadRequest();

        var typeName = parts[0];
        var methodName = parts[1];

        var type = Options.ExposedTypes.FirstOrDefault(t => t.Name == typeName);
        if (type == null) return Results.NotFound();

        var actionMethod = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        if (actionMethod == null) return Results.NotFound();

        var parameters = actionMethod.GetParameters();
        if (parameters.Length != request.Args.Count) return Results.BadRequest();

        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var arg = request.Args[i];
            args[i] = arg.Deserialize(paramType, JsonOptions);
        }

        var instance = serviceProvider.GetRequiredService(type);
        var actionValue = actionMethod.Invoke(instance, args);
        var finalValue = await AwaitIfTask(actionValue);

        if (finalValue == null)
        {
            return Results.NoContent();
        }

        return Results.Json(finalValue, JsonOptions);
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

    private async Task HandleWebSocket(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }
        if (EventWebSocket != null)
        {
            await EventWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
        EventWebSocket = await context.WebSockets.AcceptWebSocketAsync();
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
        finally
        {
            EventWebSocket = null;
        }
    }
}
