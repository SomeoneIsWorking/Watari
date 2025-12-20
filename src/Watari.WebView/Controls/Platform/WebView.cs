using Watari.Controls.MacOS;

namespace Watari.Controls.Platform;

public class WebView
{
    public MacOS.WebView? MacOS;
    public WebView()
    {
        MacOS = new MacOS.WebView();
    }
    public bool Navigate(string url) => MacOS!.Navigate(url);
    public bool Eval(string js) => MacOS!.Eval(js);
    public void Destroy() => MacOS!.Destroy();
}
