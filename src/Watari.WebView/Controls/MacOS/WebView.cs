using System.Runtime.InteropServices;
using Watari.Bridge.MacOS;
using Watari.Controls.Interfaces;

namespace Watari.Controls.MacOS;

public class WebView : IWebView
{
    private WebViewBridge.ConsoleCallbackDelegate? _consoleCallbackDelegate;

    public IntPtr Handle { get; private set; } = IntPtr.Zero;

    public event Action<string, string> ConsoleMessage = delegate { };

    public WebView()
    {
        _consoleCallbackDelegate = (level, message) =>
        {
            ConsoleMessage(level, message);
        };
        var ptr = Marshal.GetFunctionPointerForDelegate(_consoleCallbackDelegate);
        Handle = WebViewBridge.Create(ptr);
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

    public void SetEnableDevTools(bool enable)
    {
        WebViewBridge.SetEnableDevTools(Handle, enable);
    }
}
