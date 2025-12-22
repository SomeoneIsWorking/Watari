using System.CommandLine;
using Watari.Commands;

namespace Watari;

public class Framework(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;

    public bool Run(string[] args)
    {
        var rootCommand = new RootCommand("Watari Framework");

        var devCommand = new Command("dev", "Run in development mode");
        devCommand.SetAction((parseResult) => new DevCommand(Options).Execute());

        var publishCommand = new Command("publish", "Publish the application");
        publishCommand.SetAction((parseResult) => new PublishCommand(Options).ExecuteAsync());

        var generateCommand = new Command("generate", "Generate TypeScript types");
        generateCommand.SetAction((parseResult) => new GenerateCommand(Options).Execute());

        rootCommand.Add(devCommand);
        rootCommand.Add(publishCommand);
        rootCommand.Add(generateCommand);

        return rootCommand.Parse(args).Invoke() == 0;
    }
}
