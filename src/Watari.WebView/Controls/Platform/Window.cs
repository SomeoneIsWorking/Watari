namespace Watari.WebView.Controls.Platform
{
    internal class Window
    {
        private readonly MacOS.Window _macWindow;

        public Window()
        {
            _macWindow = new MacOS.Window();
        }

        public void SetContent(WebView webview)
        {
            _macWindow.SetContent(webview.MacOS!);
        }
    }
}
