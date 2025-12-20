using System.Runtime.CompilerServices;

namespace Watari;

public class Framework
{
    public Types Types { get; } = new Types();
    public Server Server { get; } = new Server();
    public FrameworkOptions Options { get; }

    public Framework([CallerFilePath] string? callerFilePath = null)
    {
        Options = new FrameworkOptions
        {
            FrontendPath = Path.GetDirectoryName(callerFilePath!)!,
            Dev = true
        };
    }

    public Framework(FrameworkOptions options)
    {
        Options = options;
    }

    public bool Run(string[] args)
    {
        if (args.Any(x => x == "-g" || x == "--generate"))
        {
            return Types.Generate();
        }

        // Initialize application (menus, Dock, activation)
        var app = new Controls.Platform.Application();
        var win = new Controls.Platform.Window();
        app.AddWindow(win, true);
        var webview = new Controls.Platform.WebView();
        win.SetContent(webview);

        var server = Server.Start(Options.Dev, Options.FrontendPath);

        app.RunLoop();
        return true;
    }
}
