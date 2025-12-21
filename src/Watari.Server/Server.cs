
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json;

namespace Watari;

public class Server(IOptions<ServerOptions> options, TypeConverter typeConverter, IServiceProvider serviceProvider)
{
    public NpmManager NpmManager { get; } = new NpmManager();
    public WebApplication WebApplication { get; private set; } = null!;
    public ServerOptions Options { get; } = options.Value;
    private TypeConverter _typeConverter = typeConverter;
    private IServiceProvider _serviceProvider = serviceProvider;

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

    private async Task<IResult> HandleRequest(HttpContext context)
    {
        var request = await JsonSerializer.DeserializeAsync<InvokeRequest>(context.Request.Body, _typeConverter.JsonOptions);
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
            args[i] = _typeConverter.ResolveInput(paramType, arg);
        }

        var instance = _serviceProvider.GetRequiredService(type);
        var result = method.Invoke(instance, args);

        object? response = _typeConverter.ResolveResponse(method.ReturnType, result);

        if (response == null)
        {
            return Results.NoContent();
        }

        return Results.Json(response, _typeConverter.JsonOptions);
    }
}
