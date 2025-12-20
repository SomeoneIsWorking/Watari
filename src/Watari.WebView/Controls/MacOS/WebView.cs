using Watari.Bridge.MacOS;

namespace Watari.Controls.MacOS;

public class WebView
{
    public IntPtr Handle { get; private set; } = IntPtr.Zero;

    public WebView()
    {
        Handle = WebViewBridge.Create();
    }
    public bool Navigate(string url)
    {
        WebViewBridge.Navigate(Handle, url ?? "about:blank");
        return true;
    }

    public bool Eval(string js)
    {
        WebViewBridge.Eval(Handle, js ?? string.Empty);
        return true;
    }

    public void Destroy() => WebViewBridge.Destroy(Handle);
}
