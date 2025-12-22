
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

    public FrameworkBuilder SetServerPort(int port)
    {
        _options.ServerPort = port;
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

    public FrameworkBuilder AddHandler<TInput, TOutput, THandler>() where THandler : class, ITypeHandler<TInput, TOutput>, new()
    {
        _options.TypeHandlers[typeof(TInput)] = new THandler();
        _options.JsonConverters.Add(new TypeHandlerConverter<TInput, TOutput>(new THandler()));
        return this;
    }

    public FrameworkBuilder Expose<T>()
    {
        _options.Services.AddSingleton(typeof(T));
        _options.ExposedTypes.Add(typeof(T));
        return this;
    }

    public FrameworkBuilder ConfigureServices(Action<ServiceCollection> configure)
    {
        configure(_options.Services);
        return this;
    }
}
