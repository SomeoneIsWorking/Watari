namespace Watari.WebView;

class Program
{

    [STAThread]
    public static int Main(string[] args)
    {
        string url = "https://www.example.com";

        // Initialize application (menus, Dock, activation)
        var app = new Controls.Platform.Application();

        var win = new Controls.Platform.Window();
        app.AddWindow(win, true);
        var webview = new Controls.Platform.WebView();
        webview.Navigate(url);
        webview.OnMessage += (m) => Console.WriteLine("[webview] " + m);
        win.SetContent(webview);

        app.RunLoop();
        return 0;
    }
}