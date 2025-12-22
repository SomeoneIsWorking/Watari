
using Watari.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Watari;

public class FrameworkBuilder
{
    private readonly FrameworkOptions _options = new();

    public FrameworkBuilder SetFrontendPath(string path)
    {
        _options.FrontendPath = path;
        return this;
    }

    public FrameworkBuilder SetDevPort(int port)
    {
        _options.DevPort = port;
        return this;
    }

    public Framework Build()
    {
        return new Framework(_options);
    }

    public FrameworkBuilder FrontendPath(string relativePath)
    {
        _options.FrontendPath = relativePath;
        return this;
    }

    public FrameworkBuilder AddHandler<THandler>() where THandler : ITypeHandler, new()
    {
        var handlerType = typeof(THandler);
        var interfaceType = handlerType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeHandler<,>));
        if (interfaceType == null)
        {
            throw new InvalidOperationException($"Type {handlerType.Name} must implement ITypeHandler<T, TS> to be used as a type handler.");
        }
        var tCSharp = interfaceType.GetGenericArguments()[0];
        var instance = new THandler();
        _options.Services.AddSingleton(typeof(ITypeHandler<>).MakeGenericType(tCSharp), instance);
        return this;
    }

    public FrameworkBuilder ConfigureServices(Action<IServiceCollection> configureServices)
    {
        _options.ConfigureServices = configureServices;
        return this;
    }

    public FrameworkBuilder Expose<T>()
    {
        _options.Services.AddScoped(typeof(T));
        _options.ExposedTypes.Add(typeof(T));
        return this;
    }

    public object SetRelativePath()
    {
        throw new NotImplementedException();
    }
}
