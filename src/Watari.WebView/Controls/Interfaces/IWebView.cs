namespace Watari.Controls.Interfaces;

public interface IWebView
{
    bool Navigate(string url);
    bool Eval(string js);
    void Destroy();
    void AddUserScript(string scriptSource, int injectionTime, bool forMainFrameOnly);
    void SetEnableDevTools(bool enable);
    event Action<string, string> ConsoleMessage;
}