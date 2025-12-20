using Watari.WebView.Bridge.MacOS;

namespace Watari.WebView.Controls.MacOS;

public class Window
{
    public IntPtr Handle { get; private set; } = IntPtr.Zero;

    public Window()
    {
        // create window and attach webview message callback
        Handle = WindowBridge.WindowBridge_CreateWindow();
    }

    public void SetContent(WebView webview)
    {
        WindowBridge.WindowBridge_SetContent(Handle, webview.Handle);
    }
}
