

using System.Diagnostics;

namespace Watari;

public class NpmManager
{
    public event EventHandler? Ready;

    public async Task StartDev(string dir, int port, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting npm dev server...");
        var tsc = new TaskCompletionSource();
        // Start npm dev server logic here
        ProcessStartInfo startInfo = new()
        {
            FileName = "npm",
            Arguments = $"run dev -- --port {port}",
            WorkingDirectory = dir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        Process process = new()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };
        process.OutputDataReceived += (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }
            if (e.Data.Contains("ready in"))
            {
                Ready?.Invoke(this, EventArgs.Empty);
                tsc.SetResult();
            }
            Console.WriteLine($"[npm stdout]: {e.Data}");
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }
            Console.WriteLine($"[npm stderr]: {e.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        cancellationToken.Register(() =>
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        });
        await tsc.Task;
    }
}