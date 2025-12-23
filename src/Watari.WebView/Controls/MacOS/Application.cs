using Watari.Bridge.MacOS;
using Watari.Controls.Interfaces;

namespace Watari.Controls.MacOS;

internal class Application : IApplication
{
    public IntPtr Handle { get; }
    public Application()
    {
        Handle = ApplicationBridge.Init();
    }

    public void SetMainWindow(IWindow window)
    {
        if (window is not Window macosWindow)
        {
            throw new ArgumentException("window must be of type MacOS.Window");
        }
        ApplicationBridge.SetMainWindow(Handle, macosWindow.Handle);
    }

    public void RunLoop() => ApplicationBridge.RunLoop(Handle);
    public void StopLoop() => ApplicationBridge.StopLoop(Handle);

    public void RunOnMainThread(Action action)
    {
        ApplicationBridge.RunOnMainThread(Handle, () => action());
    }

    public void AddMenuItem(string title)
    {
        ApplicationBridge.AddMenuItem(Handle, title);
    }

    public string? OpenFileDialog(string allowedExtensions)
    {
        return ApplicationBridge.OpenFileDialog(Handle, allowedExtensions);
    }
}
