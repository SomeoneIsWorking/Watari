

using CliWrap;
using Microsoft.Extensions.Logging;

namespace Watari;

public class NpmManager(string dir, int port, ILogger<NpmManager> logger)
{
    public async Task StartDevAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting npm dev server...");
        var tsc = new TaskCompletionSource();

        // Script to start npm dev server with port and monitor parent process
        // To prevent orphaned processes if the parent crashes
        var watchdogScript = $@"
            const {{ spawn }} = require('child_process');
            const child = spawn('npm', ['run', 'dev', '--', '--port', '{port}'], {{ stdio: 'inherit', shell: true }});
            setInterval(() => {{
                try {{ process.kill({Environment.ProcessId}, 0); }} 
                catch (e) {{ child.kill(); process.exit(); }}
            }}, 500);";

        _ = Cli.Wrap("node")
            .WithArguments(["-e", watchdogScript])
            .WithWorkingDirectory(dir)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line =>
            {
                logger.LogInformation("[npm stdout]: {Line}", line);
                if (line.Contains("ready in"))
                {
                    tsc.SetResult();
                }
            }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line =>
            {
                logger.LogError("[npm stderr]: {Line}", line);
                tsc.SetException(new Exception(line));
            }))
            .ExecuteAsync(default, cancellationToken);
        await tsc.Task;
    }
}