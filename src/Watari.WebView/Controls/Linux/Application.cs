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

    public void RunOnMainThread(Action action)
    {
        ApplicationBridge.g_idle_add((data) => { action(); return 0; }, IntPtr.Zero);
    }

    public void AddMenuItem(string title)
    {
        // No op for GTK
    }

    public string? OpenFileDialog(string allowedExtensions)
    {
        // TODO: Implement GTK file dialog
        return null;
    }

    public void InitAudio(double sampleRate)
    {
        // TODO: Implement audio initialization
    }

    public void PlayAudio(short[] samples)
    {
        // TODO: Implement audio playback
    }
}