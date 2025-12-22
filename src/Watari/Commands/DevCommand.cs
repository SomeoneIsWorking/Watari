using Microsoft.Extensions.DependencyInjection;

namespace Watari.Commands;

public class DevCommand(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;

    public void Execute()
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
        var context = new WatariContext { };
        services.AddSingleton(context);

        var serviceProvider = services.BuildServiceProvider();
        var server = serviceProvider.GetRequiredService<Server>();
        Task serverStartTask = server.Start();

        // Initialize application early for DI
        var app = new Controls.Platform.Application();
        var win = new Controls.Platform.Window();
        context.MainWindow = win;
        // Initialize application (menus, Dock, activation)
        app.AddWindow(win, true);
        var webview = new Controls.Platform.WebView();
        win.SetContent(webview);
        serverStartTask.Wait();

        if (Options.Dev)
        {
            webview.Navigate($"http://localhost:{Options.DevPort}");
        }

        // Inject watari_invoke as user script
        var invokeScript = $"window.watari_invoke = async function(method, ...args) {{ const response = await fetch('http://localhost:{Options.ServerPort}/invoke', {{ method: 'POST', headers: {{ 'Content-Type': 'application/json' }}, body: JSON.stringify({{ method, args }}) }}); return await response.json(); }};";
        webview.AddUserScript(invokeScript, 0, true);

        app.RunLoop();
    }
}