namespace Watari.Controls.Platform;

public class Window
{
    public MacOS.Window MacOS  { get; }

    public Window()
    {
        MacOS = new MacOS.Window();
    }

    public void SetContent(WebView webview)
    {
        MacOS.SetContent(webview.MacOS!);
    }
}
