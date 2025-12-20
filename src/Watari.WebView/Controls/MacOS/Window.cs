using Watari.Bridge.MacOS;
using Watari.Controls.Interfaces;

namespace Watari.Controls.MacOS;

public class Window : IWindow
{
    public IntPtr Handle { get; private set; } = IntPtr.Zero;

    public Window()
    {
        Handle = WindowBridge.WindowBridge_CreateWindow();
    }

    public void SetContent(IWebView webview)
    {
        if (webview is not WebView macosWebView)
        {
            throw new ArgumentException("webview must be of type MacOS.WebView");
        }
        WindowBridge.WindowBridge_SetContent(Handle, macosWebView.Handle);
    }
}
