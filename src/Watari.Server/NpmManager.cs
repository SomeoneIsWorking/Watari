

using System.Diagnostics;

namespace Watari;

public class NpmManager
{
    public void StartDev(string dir, CancellationToken cancellationToken = default)
    {
        // Start npm dev server logic here
        ProcessStartInfo startInfo = new()
        {
            FileName = "npm",
            Arguments = "run dev",
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
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine($"[npm stdout]: {e.Data}");
            }
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine($"[npm stderr]: {e.Data}");
            }
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }
}