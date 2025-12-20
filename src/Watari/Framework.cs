using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Watari;

public class Framework(FrameworkOptions options)
{
    public Types Types { get; } = new Types();
    public Server Server { get; } = new Server();
    public FrameworkOptions Options { get; } = options;

    public bool Run(string[] args)
    {
        if (args.Any(x => x == "-g" || x == "--generate"))
        {
            return Types.Generate();
        }

        Server.Start(new ServerOptions
        {
            Dev = Options.Dev,
            DevPort = Options.DevPort,
            FrontendPath = Options.FrontendPath
        }).GetAwaiter().GetResult();

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

        app.RunLoop();
        return true;
    }
}
