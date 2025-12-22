using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;
using Microsoft.Extensions.DependencyInjection;
using Watari.Types;

namespace Watari.Commands;

public class PublishCommand(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;

    public async Task ExecuteAsync()
    {
        var services = Options.Services;
        services.AddSingleton(new TypeConverter(Options.JsonConverters));
        var provider = services.BuildServiceProvider();

        new TypeGenerator(new TypeGeneratorOptions
        {
            OutputPath = Options.FrontendPath,
            ExposedTypes = Options.ExposedTypes,
            Provider = provider
        }).Generate();

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