
namespace Watari.Controls.Platform;

public class Application
{
    private readonly MacOS.Application? MacOS;
    private readonly List<Window> _windows = [];
    public IReadOnlyList<Window> Windows => _windows;
    public Window? MainWindow { get; private set; }

    public Application()
    {
        MacOS = new MacOS.Application();
    }

    public void RunLoop() => MacOS!.RunLoop();
    public void StopLoop() => MacOS!.StopLoop();

    public void AddWindow(Window window, bool mainWindow)
    {
        _windows.Add(window);
        if (mainWindow)
        {
            MacOS!.SetMainWindow(window.MacOS);
            MainWindow = window;
        }
    }
}
