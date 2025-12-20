using Watari.Controls.Interfaces;

namespace Watari.Controls.Platform;

public class WebView
{
    public IWebView WebViewImpl { get; }
    public WebView()
    {
        WebViewImpl = new MacOS.WebView();
    }
    public bool Navigate(string url) => WebViewImpl.Navigate(url);
    public bool Eval(string js) => WebViewImpl.Eval(js);
    public void Destroy() => WebViewImpl.Destroy();
    public void AddUserScript(string scriptSource, int injectionTime, bool forMainFrameOnly) => WebViewImpl.AddUserScript(scriptSource, injectionTime, forMainFrameOnly);
}
