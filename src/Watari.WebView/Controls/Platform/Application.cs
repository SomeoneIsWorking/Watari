
namespace Watari.WebView.Controls.Platform
{
    internal class Application
    {
        private readonly MacOS.Application _macApp;
        private readonly List<Window> _windows = new List<Window>();

        public Application()
        {
            _macApp = new MacOS.Application();
        }

        public void RunLoop() => _macApp.RunLoop();
        public void StopLoop() => _macApp.StopLoop();

        internal void AddWindow(Window window)
        {
            _windows.Add(window);
        }
    }
}
