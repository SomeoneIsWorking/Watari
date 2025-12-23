
using System.Runtime.InteropServices;
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _application = new MacOS.Application();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _application = new Linux.Application();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _application = new Windows.Application();
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform");
        }
    }

    public void RunLoop() => _application.RunLoop();
    public void StopLoop() => _application.StopLoop();

    public void RunOnMainThread(Action action) => _application.RunOnMainThread(action);

    public void AddMenuItem(string title) => _application.AddMenuItem(title);

    public string? OpenFileDialog(string allowedExtensions) => _application.OpenFileDialog(allowedExtensions);

    public void AddWindow(Window window, bool mainWindow)
    {
        _windows.Add(window);
        if (mainWindow)
        {
            _application.SetMainWindow(window.WindowImpl);
            MainWindow = window;
        }
        window.Application = _application;
    }
}
