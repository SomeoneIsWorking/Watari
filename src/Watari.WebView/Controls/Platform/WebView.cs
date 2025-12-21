using System.Runtime.InteropServices;
using Watari.Controls.Interfaces;

namespace Watari.Controls.Platform;

public class WebView
{
    public IWebView WebViewImpl { get; }
    public WebView()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            WebViewImpl = new MacOS.WebView();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            WebViewImpl = new Linux.WebView();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WebViewImpl = new Windows.WebView();
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform");
        }
    }
    public bool Navigate(string url) => WebViewImpl.Navigate(url);
    public bool Eval(string js) => WebViewImpl.Eval(js);
    public void Destroy() => WebViewImpl.Destroy();
    public void AddUserScript(string scriptSource, int injectionTime, bool forMainFrameOnly) => WebViewImpl.AddUserScript(scriptSource, injectionTime, forMainFrameOnly);
}
