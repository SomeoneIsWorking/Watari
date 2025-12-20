namespace Watari.Controls.Interfaces;

public interface IWebView
{
    bool Navigate(string url);
    bool Eval(string js);
    void Destroy();
}