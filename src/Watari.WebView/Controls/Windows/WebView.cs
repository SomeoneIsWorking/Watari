using Microsoft.Web.WebView2.Core;
using Watari.Controls.Interfaces;

namespace Watari.Controls.Windows;

public class WebView : IWebView
{
    private CoreWebView2? _coreWebView2;
    private CoreWebView2Controller? _controller;

    public event Action<string, string> ConsoleMessage = delegate { };

    public WebView()
    {
    }

    public async void SetParentHandle(IntPtr handle)
    {
        var environment = await CoreWebView2Environment.CreateAsync();
        _controller = await environment.CreateCoreWebView2ControllerAsync(handle);
        _coreWebView2 = _controller.CoreWebView2;
    }

    public bool Navigate(string url)
    {
        if (_coreWebView2 != null)
        {
            _coreWebView2.Navigate(url);
            return true;
        }
        return false;
    }

    public bool Eval(string js)
    {
        if (_coreWebView2 != null)
        {
            _coreWebView2.ExecuteScriptAsync(js);
            return true;
        }
        return false;
    }

    public void Destroy()
    {
        _controller?.Close();
        _coreWebView2 = null;
        _controller = null;
    }

    public void AddUserScript(string scriptSource, int injectionTime, bool forMainFrameOnly)
    {
        if (_coreWebView2 != null)
        {
            _coreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(scriptSource);
        }
    }
}