using Watari.WebView.Controls.MacOS;

namespace Watari.WebView.Controls.Platform;

public class WebView
{
    public MacOS.WebView? MacOS;
    public Action<object>? OnMessage { get; internal set; }

    public WebView()
    {
        MacOS = new MacOS.WebView();
    }
    public bool Navigate(string url) => MacOS != null && MacOS.Navigate(url);
    public bool Eval(string js) => MacOS != null && MacOS.Eval(js);
    public void Destroy() => MacOS?.Destroy();
}
