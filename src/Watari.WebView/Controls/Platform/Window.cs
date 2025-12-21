using System.Runtime.InteropServices;
using Watari.Controls.Interfaces;

namespace Watari.Controls.Platform;

public class Window
{
    public IWindow WindowImpl { get; }

    public Window()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            WindowImpl = new MacOS.Window();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            WindowImpl = new Linux.Window();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WindowImpl = new Watari.Controls.Windows.Window();
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform");
        }
    }

    public void SetContent(WebView webview)
    {
        WindowImpl.SetContent(webview.WebViewImpl);
    }
}
