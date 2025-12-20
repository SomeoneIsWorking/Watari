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
            return Types.Generate(new TypeGeneratorOptions
            {
                OutputPath = Options.FrontendPath,
                ExposedTypes = Options.ExposedTypes,
                Handlers = Options.Handlers

            });
        }

        Server.Start(new ServerOptions
        {
            Dev = Options.Dev,
            DevPort = Options.DevPort,
            ServerPort = Options.ServerPort,
            FrontendPath = Options.FrontendPath,
            ExposedTypes = Options.ExposedTypes,
            Handlers = Options.Handlers
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

        // Inject watari_invoke as user script
        var invokeScript = $"window.watari_invoke = async function(method, ...args) {{ const response = await fetch('http://localhost:{Options.ServerPort}/invoke', {{ method: 'POST', headers: {{ 'Content-Type': 'application/json' }}, body: JSON.stringify({{ method, args }}) }}); return await response.json(); }};";
        webview.AddUserScript(invokeScript, 0, true);

        app.RunLoop();
        return true;
    }
}
