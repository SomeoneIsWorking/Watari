
using Watari.Controls.Interfaces;

namespace Watari.Controls.Platform;

public class Application
{
    private readonly IApplication _application;
    private readonly List<Window> _windows = [];
    public IReadOnlyList<Window> Windows => _windows;
    public Window? MainWindow { get; private set; }

    public Application()
    {
        _application = new MacOS.Application();
    }

    public void RunLoop() => _application.RunLoop();
    public void StopLoop() => _application.StopLoop();

    public void AddWindow(Window window, bool mainWindow)
    {
        _windows.Add(window);
        if (mainWindow)
        {
            _application.SetMainWindow(window.WindowImpl);
            MainWindow = window;
        }
    }
}
