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
        WebViewBridge.Navigate(Handle, url);
        return true;
    }

    public bool Eval(string js)
    {
        WebViewBridge.Eval(Handle, js);
        return true;
    }

    public void Destroy() => WebViewBridge.Destroy(Handle);
}
