using Watari.Bridge.Linux;
using Watari.Controls.Interfaces;

namespace Watari.Controls.Linux;

internal class Application : IApplication
{
    public IntPtr Handle { get; } = IntPtr.Zero;

    public Application()
    {
        ApplicationBridge.gtk_init(IntPtr.Zero, IntPtr.Zero);
    }

    public void SetMainWindow(IWindow window)
    {
        // No op for GTK
    }

    public void RunLoop() => ApplicationBridge.gtk_main();

    public void StopLoop() => ApplicationBridge.gtk_main_quit();
}