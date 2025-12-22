

using CliWrap;
using CliWrap.EventStream;

namespace Watari;

public class NpmManager(string dir, int port)
{
    public event EventHandler? Ready;

    public async Task StartDevAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting npm dev server...");
        var tsc = new TaskCompletionSource();
        // Start npm dev server logic here
        await foreach (var evt in Cli.Wrap("npm")
            .WithArguments($"run dev -- --port {port}")
            .WithWorkingDirectory(dir)
            .ListenAsync(cancellationToken))
        {
            switch (evt)
            {
                case StartedCommandEvent started:
                    break;
                case StandardOutputCommandEvent output:
                    Console.WriteLine($"[npm stdout]: {output.Text}");
                    if (output.Text.Contains("ready in"))
                    {
                        Ready?.Invoke(this, EventArgs.Empty);
                        tsc.SetResult();
                    }
                    break;
                case StandardErrorCommandEvent error:
                    Console.Error.WriteLine($"[npm stderr]: {error.Text}");
                    break;
            }
            if (tsc.Task.IsCompleted) break;
        }
        await tsc.Task;
    }
}