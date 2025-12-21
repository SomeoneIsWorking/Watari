using Microsoft.Extensions.DependencyInjection;
using Watari.Types;

namespace Watari;

public class Framework(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;

    public bool Run(string[] args)
    {
        var services = Options.Services;
        services.AddSingleton<TypeConverter>();
        services.Configure<ServerOptions>(serverOptions =>
        {
            serverOptions.Dev = Options.Dev;
            serverOptions.DevPort = Options.DevPort;
            serverOptions.ServerPort = Options.ServerPort;
            serverOptions.FrontendPath = Options.FrontendPath;
            serverOptions.ExposedTypes = Options.ExposedTypes;
        });
        Options.ConfigureServices?.Invoke(services);
        services.AddTransient<Server>();

        if (args.Any(x => x == "-g" || x == "--generate"))
        {
            var provider = services.BuildServiceProvider();
            return new TypeGenerator(new TypeGeneratorOptions
            {
                OutputPath = Options.FrontendPath,
                ExposedTypes = Options.ExposedTypes,
                Provider = provider
            }).Generate();
        }

        var serviceProvider = services.BuildServiceProvider();
        var server = serviceProvider.GetRequiredService<Server>();
        server.Start().GetAwaiter().GetResult();

        // Initialize application (menus, Dock, activation)
        var app = new Controls.Platform.Application();
        var win = new Controls.Platform.Window();
        app.AddWindow(win, true);
        var webview = new Controls.Platform.WebView();
        win.SetContent(webview);

        if (Options.Dev)
        {
            webview.Navigate($"http://localhost:{Options.DevPort}");
        }

        // Inject watari_invoke as user script
        var invokeScript = $"window.watari_invoke = async function(method, ...args) {{ const response = await fetch('http://localhost:{Options.ServerPort}/invoke', {{ method: 'POST', headers: {{ 'Content-Type': 'application/json' }}, body: JSON.stringify({{ method, args }}) }}); return await response.json(); }};";
        webview.AddUserScript(invokeScript, 0, true);

        app.RunLoop();
        return true;
    }
}
