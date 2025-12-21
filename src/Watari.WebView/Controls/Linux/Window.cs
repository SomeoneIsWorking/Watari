using Watari.Bridge.Linux;
using Watari.Controls.Interfaces;

namespace Watari.Controls.Linux;

public class Window : IWindow
{
    public IntPtr Handle { get; private set; } = IntPtr.Zero;

    public Window()
    {
        Handle = WindowBridge.gtk_window_new(0); // GTK_WINDOW_TOPLEVEL
        WindowBridge.gtk_widget_show(Handle);
    }

    public void SetContent(IWebView webview)
    {
        if (webview is not WebView linuxWebView)
        {
            throw new ArgumentException("webview must be of type Linux.WebView");
        }
        WindowBridge.gtk_container_add(Handle, linuxWebView.Handle);
    }
}