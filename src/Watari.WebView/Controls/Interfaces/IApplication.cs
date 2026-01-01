namespace Watari.Controls.Interfaces;

public interface IApplication
{
    void SetMainWindow(IWindow window);
    void RunLoop();
    void StopLoop();
    void RunOnMainThread(Action action);
    void AddMenuItem(string title);
    string? OpenFileDialog(string allowedExtensions);
    void InitAudio(double sampleRate);
    void PlayAudio(short[] samples);
}
