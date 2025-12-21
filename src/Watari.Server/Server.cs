
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Watari;

public class Server
{
    public NpmManager NpmManager { get; } = new NpmManager();
    public WebApplication WebApplication { get; private set; } = null!;
    public ServerOptions Options { get; }
    private JsonSerializerOptions _jsonOptions;

    public Server(ServerOptions options)
    {
        Options = options;
        _jsonOptions = BuildSerializerOptions();
    }

    private JsonSerializerOptions BuildSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        foreach (var kvp in Options.Handlers)
        {
            var type = kvp.Key;
            var handler = kvp.Value;
            var interfaceType = handler.GetType().GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeHandler<,>));
            var genericArgs = interfaceType.GetGenericArguments();
            var converterType = typeof(TypeHandlerConverter<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
            var converter = Activator.CreateInstance(converterType, handler);
            options.Converters.Add((JsonConverter)converter!);
        }
        return options;
    }

    public async Task Start()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddCors();
        WebApplication webApplication = builder.Build();
        webApplication.Urls.Add($"http://localhost:{Options.ServerPort}");
        webApplication.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        webApplication.MapPost("/invoke", (Delegate)HandleRequest);
        WebApplication = webApplication;

        if (Options.Dev)
        {
            var start = webApplication.StartAsync(Options.CancellationToken);
            await NpmManager.StartDev(Options.FrontendPath, Options.DevPort, Options.CancellationToken);
            await start;
        }
        else
        {
            await webApplication.StartAsync(Options.CancellationToken);
        }
    }

    public object? ParseInput(string json, Type type)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        return ResolveInput(type, jsonElement);
    }

    public string SerializeOutput(object? value)
    {
        var type = value?.GetType() ?? typeof(void);
        var resolved = ResolveResponse(type, value);
        return JsonSerializer.Serialize(resolved, _jsonOptions);
    }

    private async Task<IResult> HandleRequest(HttpContext context)
    {
        var request = await JsonSerializer.DeserializeAsync<InvokeRequest>(context.Request.Body, _jsonOptions);
        if (request == null) return Results.BadRequest();

        var parts = request.Method.Split('.');
        if (parts.Length != 2) return Results.BadRequest();

        var typeName = parts[0];
        var methodName = parts[1];

        var type = Options.ExposedTypes.FirstOrDefault(t => t.Name == typeName);
        if (type == null) return Results.NotFound();

        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        if (method == null) return Results.NotFound();

        var parameters = method.GetParameters();
        if (parameters.Length != request.Args.Count) return Results.BadRequest();

        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var arg = request.Args[i];
            args[i] = ResolveInput(paramType, arg);
        }

        var instance = Activator.CreateInstance(type);
        var result = method.Invoke(instance, args);

        object? response = ResolveResponse(method.ReturnType, result);

        if (response == null)
        {
            return Results.NoContent();
        }

        return Results.Json(response, _jsonOptions);
    }

    private object? ResolveResponse(Type type, object? value)
    {
        if (value == null) return null;

        if (type == typeof(void))
        {
            return null; // Though void methods return NoContent earlier
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var task = (Task)value;
            var resultProperty = task.GetType().GetProperty("Result");
            var actualResult = resultProperty?.GetValue(task);
            var innerType = type.GetGenericArguments()[0];
            return ResolveResponse(innerType, actualResult);
        }
        else if (type == typeof(Task))
        {
            var task = (Task)value;
            return null; // Indicates NoContent
        }
        else
        {
            return value;
        }
    }

    private object? ResolveInput(Type type, JsonElement json)
    {
        return JsonSerializer.Deserialize(json.GetRawText(), type, _jsonOptions);
    }
}
