

using CliWrap;

namespace Watari;

public class NpmManager(string dir, int port)
{
    public async Task StartDevAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting npm dev server...");

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
                    tsc.SetResult();
                }
            }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line =>
            {
                tsc.SetException(new Exception(line));
            }))
            .ExecuteAsync(default, cancellationToken);
        await tsc.Task;
    }
}