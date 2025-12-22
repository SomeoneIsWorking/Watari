using Watari.Bridge.Linux;
using Watari.Controls.Interfaces;

namespace Watari.Controls.Linux;

public class WebView : IWebView
{
    public IntPtr Handle { get; private set; } = IntPtr.Zero;

    public event Action<string, string>? ConsoleMessage;

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
        WebViewBridge.Eval(Handle, js, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        return true;
    }

    public void Destroy() => WebViewBridge.Destroy(Handle);

    public void AddUserScript(string scriptSource, int injectionTime, bool forMainFrameOnly)
    {
        IntPtr manager = WebViewBridge.webkit_web_view_get_user_content_manager(Handle);
        int inj_time = injectionTime == 0 ? 0 : 1;
        int inj_frames = forMainFrameOnly ? 1 : 0;
        IntPtr script = WebViewBridge.webkit_user_script_new(scriptSource, inj_time, inj_frames, IntPtr.Zero, IntPtr.Zero);
        WebViewBridge.webkit_user_content_manager_add_script(manager, script);
    }
}