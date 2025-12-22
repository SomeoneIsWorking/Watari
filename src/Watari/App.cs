using System;
using Microsoft.Extensions.DependencyInjection;

namespace Watari;

public class App
{
    public static void Start(bool dev, ServiceProvider serviceProvider)
    {
        var server = serviceProvider.GetRequiredService<Server>();
        var context = serviceProvider.GetRequiredService<WatariContext>();
        var options = context.Options;
        var waitTask = server.StartAsync();
        if (dev)
        {
            var devTask = new NpmManager(CliUtils.JoinPath("frontend"), options.DevPort)
                .StartDevAsync();
            waitTask = Task.WhenAll(waitTask, devTask);
        }
        // Initialize application early for DI
        context.Application = new Controls.Platform.Application();
        context.MainWindow = new Controls.Platform.Window();
        // Initialize application (menus, Dock, activation)
        context.Application.AddWindow(context.MainWindow, true);
        context.WebView = new Controls.Platform.WebView();
        context.WebView.ConsoleMessage += (level, message) =>
        {
            Console.WriteLine($"[{level.ToUpper()}] {message}");
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

        // Inject watari_invoke as user script
        var invokeScript = $"window.watari_invoke = async function(method, ...args) {{ const response = await fetch('http://localhost:{options.ServerPort}/invoke', {{ method: 'POST', headers: {{ 'Content-Type': 'application/json' }}, body: JSON.stringify({{ method, args }}) }}); return await response.json(); }};";
        context.WebView.AddUserScript(invokeScript, 0, true);

        context.Application.RunLoop();
    }
}