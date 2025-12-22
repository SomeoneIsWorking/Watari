using System;
using System.IO;
using System.Reflection;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;

namespace Watari.Commands;

public class PublishCommand(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;

    public async Task ExecuteAsync()
    {
        // Build frontend if exists
        await Cli.Wrap("npm")
            .WithArguments("run build")
            .WithWorkingDirectory(Options.FrontendPath)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine))
            .ExecuteBufferedAsync();

        await Cli.Wrap("dotnet")
            .WithArguments($"publish --output dist")
            .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.Error.WriteLine))
            .ExecuteBufferedAsync();

        // Create .published marker file next to the executable
        File.WriteAllText(Path.Combine("dist", ".published"), "");
    }
}