using Watari.Bridge.MacOS;

namespace Watari.Controls.MacOS;

internal class Application
{
    public IntPtr Handle { get; }
    public Application()
    {
        Handle = AppliationBridge.Init();
    }

    public void SetMainWindow(Window window)
    {
        AppliationBridge.SetMainWindow(Handle, window.Handle);
    }

    public void RunLoop() => AppliationBridge.RunLoop(Handle);
    public void StopLoop() => AppliationBridge.StopLoop(Handle);
}
