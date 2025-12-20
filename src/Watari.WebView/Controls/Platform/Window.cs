using Watari.Controls.Interfaces;

namespace Watari.Controls.Platform;

public class Window
{
    public IWindow WindowImpl { get; }

    public Window()
    {
        WindowImpl = new MacOS.Window();
    }

    public void SetContent(WebView webview)
    {
        WindowImpl.SetContent(webview.WebViewImpl);
    }
}
