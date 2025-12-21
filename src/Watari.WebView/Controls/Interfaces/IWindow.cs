namespace Watari.Controls.Interfaces;

public interface IWindow
{
    void SetContent(IWebView webview);
    void Move(int x, int y);
    (int x, int y) GetPosition();
}
