
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Watari;

public class Server
{
    public NpmManager NpmManager { get; } = new NpmManager();
    public WebApplication WebApplication { get; private set; } = null!;

    public async Task Start(ServerOptions options)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddCors();
        WebApplication webApplication = builder.Build();
        webApplication.Urls.Add($"http://localhost:{options.ServerPort}");
        webApplication.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        webApplication.MapPost("/invoke", async (HttpContext context) =>
        {
            return await HandleRequest(options, context);
        });
        WebApplication = webApplication;

        if (options.Dev)
        {
            var start = webApplication.StartAsync(options.CancellationToken);
            await NpmManager.StartDev(options.FrontendPath, options.DevPort, options.CancellationToken);
            await start;
        }
        else
        {
            await webApplication.StartAsync(options.CancellationToken);
        }
    }

    private static async Task<IResult> HandleRequest(ServerOptions options, HttpContext context)
    {
        var request = await JsonSerializer.DeserializeAsync<InvokeRequest>(context.Request.Body);
        if (request == null) return Results.BadRequest();

        var parts = request.Method.Split('.');
        if (parts.Length != 2) return Results.BadRequest();

        var typeName = parts[0];
        var methodName = parts[1];

        var type = options.ExposedTypes.FirstOrDefault(t => t.Name == typeName);
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
            if (options.Handlers.TryGetValue(paramType, out var handler))
            {
                var tsType = handler.GetType().GetGenericArguments()[1];
                var tsValue = JsonSerializer.Deserialize(arg.GetRawText(), tsType);
                args[i] = handler.FromTypeScript(tsValue);
            }
            else
            {
                args[i] = JsonSerializer.Deserialize(arg.GetRawText(), paramType);
            }
        }

        var instance = Activator.CreateInstance(type);
        var result = method.Invoke(instance, args);

        object? response = await ResolveResponse(options, method.ReturnType, result);

        if (response == null)
        {
            return Results.NoContent();
        }

        return Results.Json(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    private static async Task<object?> ResolveResponse(ServerOptions options, Type type, object? value)
    {
        if (value == null) return null;

        if (type == typeof(void))
        {
            return null; // Though void methods return NoContent earlier
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var task = (Task)value;
            await task;
            var resultProperty = task.GetType().GetProperty("Result");
            var actualResult = resultProperty?.GetValue(task);
            var innerType = type.GetGenericArguments()[0];
            return await ResolveResponse(options, innerType, actualResult);
        }
        else if (type == typeof(Task))
        {
            var task = (Task)value;
            await task;
            return null; // Indicates NoContent
        }
        else if (options.Handlers.TryGetValue(type, out var handler))
        {
            var tsValue = handler.ToTypeScript(value);
            var tsType = handler.GetType().GetGenericArguments()[1];
            return await ResolveResponse(options, tsType, tsValue);
        }
        else
        {
            return value;
        }
    }
}
