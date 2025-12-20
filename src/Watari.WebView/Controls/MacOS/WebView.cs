using Watari.Bridge.MacOS;
using Watari.Controls.Interfaces;

namespace Watari.Controls.MacOS;

public class WebView : IWebView
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

    public void AddUserScript(string scriptSource, int injectionTime, bool forMainFrameOnly)
    {
        WebViewBridge.AddUserScript(Handle, scriptSource, injectionTime, forMainFrameOnly);
    }
}
