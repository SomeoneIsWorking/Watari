namespace Watari.Controls.Interfaces;

public interface IWebView
{
    bool Navigate(string url);
    bool Eval(string js);
    void Destroy();
    void AddUserScript(string scriptSource, int injectionTime, bool forMainFrameOnly);
    event Action<string, string>? ConsoleMessage;
}