

using CliWrap;
using CliWrap.EventStream;

namespace Watari;

public class NpmManager(string dir, int port)
{
    private CancellationTokenSource? _cts;

    public event EventHandler? Ready;

    public async Task StartDevAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting npm dev server...");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var tsc = new TaskCompletionSource();
        // Start npm dev server logic here
        _ = Cli.Wrap("npm")
            .WithArguments($"run dev -- --port {port}")
            .WithWorkingDirectory(dir)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line =>
            {
                Console.WriteLine($"[npm stdout]: {line}");
                if (line.Contains("ready in"))
                {
                    Ready?.Invoke(this, EventArgs.Empty);
                    tsc.SetResult();
                }
            }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line =>
            {
                Console.Error.WriteLine($"[npm stderr]: {line}");
            }))
            .ExecuteAsync(_cts.Token);
        await tsc.Task;
    }

    public void Stop()
    {
        _cts?.Cancel();
    }
}