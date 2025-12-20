
using System.Runtime.CompilerServices;

namespace Watari;

public class FrameworkBuilder
{
    private readonly FrameworkOptions _options = new();

    public FrameworkBuilder SetDev(bool dev)
    {
        _options.Dev = dev;
        return this;
    }

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

    public FrameworkBuilder SetFrontendPathRelative(string relativePath, [CallerFilePath] string callerFilePath = "")
    {
        string callerDirectory = Path.GetDirectoryName(callerFilePath)!;
        string fullPath = Path.GetFullPath(Path.Combine(callerDirectory, relativePath));
        _options.FrontendPath = fullPath;
        return this;
    }

    public FrameworkBuilder AddHandler<TCSharp, TTypeScript>(ITypeHandler<TCSharp, TTypeScript> handler)
    {
        _options.Handlers[typeof(TCSharp)] = handler;
        return this;
    }

    public FrameworkBuilder Expose<T>()
    {
        _options.ExposedTypes.Add(typeof(T));
        return this;
    }
}
