using Watari.Bridge.MacOS;
using Watari.Controls.Interfaces;

namespace Watari.Controls.MacOS;

internal class Application : IApplication
{
    public IntPtr Handle { get; }
    public Application()
    {
        Handle = AppliationBridge.Init();
    }

    public void SetMainWindow(IWindow window)
    {
        if (window is not Window macosWindow)
        {
            throw new ArgumentException("window must be of type MacOS.Window");
        }
        AppliationBridge.SetMainWindow(Handle, macosWindow.Handle);
    }

    public void RunLoop() => AppliationBridge.RunLoop(Handle);
    public void StopLoop() => AppliationBridge.StopLoop(Handle);
}
