using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;

namespace Watari.Commands;

public class PublishCommand(FrameworkOptions options, ILogger<PublishCommand> logger)
{
    public FrameworkOptions Options { get; } = options;

    public async Task ExecuteAsync()
    {
        // Build frontend if exists
        await Cli.Wrap("npm")
            .WithArguments("run build")
            .WithWorkingDirectory(Options.FrontendPath)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line => logger.LogInformation("[npm build] {Line}", line)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line => logger.LogError("[npm build] {Line}", line)))
            .ExecuteBufferedAsync();

        await Cli.Wrap("dotnet")
            .WithArguments($"publish --output dist")
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line => logger.LogInformation("[dotnet publish] {Line}", line)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line => logger.LogError("[dotnet publish] {Line}", line)))
            .ExecuteBufferedAsync();

        // Copy frontend dist to published folder
        var frontendDistPath = Path.Combine(Options.FrontendPath, "dist");
        var publishedFrontendPath = Path.Combine("dist", "wwwroot");
        CopyDirectory(frontendDistPath, publishedFrontendPath);

        // Create .published marker file next to the executable
        File.WriteAllText(Path.Combine("dist", ".published"), "");
    }

    private static void CopyDirectory(string frontendDistPath, string publishedFrontendPath)
    {
        if (Directory.Exists(frontendDistPath))
        {
            if (Directory.Exists(publishedFrontendPath))
            {
                Directory.Delete(publishedFrontendPath, true);
            }
            Directory.CreateDirectory(publishedFrontendPath);
            foreach (var file in Directory.GetFiles(frontendDistPath, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(frontendDistPath, file);
                var destPath = Path.Combine(publishedFrontendPath, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                File.Copy(file, destPath);
            }
        }
    }
}