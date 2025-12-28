using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Watari;

public class App
{
    private static NpmManager? _npmManager;
    private static CancellationTokenSource? _cts;

    public static void Start(bool dev, ServiceProvider serviceProvider)
    {
        var server = serviceProvider.GetRequiredService<Server>();
        var context = serviceProvider.GetRequiredService<WatariContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<App>>();
        context.Server = server;
        var options = context.Options;
        var waitTask = server.StartAsync();
        if (dev)
        {
            _cts = new CancellationTokenSource();
            _npmManager = new NpmManager(CliUtils.JoinPath("frontend"), options.DevPort, serviceProvider.GetRequiredService<ILogger<NpmManager>>());
            var devTask = _npmManager.StartDevAsync(_cts.Token);
            waitTask = Task.WhenAll(waitTask, devTask);
        }
        // Initialize application early for DI
        context.Application = new Controls.Platform.Application();
        context.MainWindow = new Controls.Platform.Window();
        // Initialize application (menus, Dock, activation)
        context.Application.AddWindow(context.MainWindow, true);
        context.WebView = new Controls.Platform.WebView();
        context.WebView.SetEnableDevTools(dev);
        context.WebView.ConsoleMessage += (level, message) =>
        {
            logger.LogInformation("[WebView] [{Level}] {Message}", level.ToUpper(), message);
        };
        context.MainWindow.SetContent(context.WebView);
        waitTask.Wait();

        if (dev)
        {
            context.WebView.Navigate($"http://localhost:{options.DevPort}");
        }
        else
        {
            context.WebView.Navigate($"http://localhost:{options.ServerPort}/index.html");
        }

        // Inject watari API as user script
        var watariScript = WatariResources.WatariJs + $"\ninitWatari({options.ServerPort});";
        context.WebView.AddUserScript(watariScript, 0, true);

        AppDomain.CurrentDomain.ProcessExit += (s, e) => _cts?.Cancel();

        context.Application.RunLoop();
    }
}