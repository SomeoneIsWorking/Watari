using System.CommandLine;
using Watari.Commands;

namespace Watari
{
    public class CommandRunner
    {
        public static bool Run(Framework framework, string[] args)
        {
            CliUtils.EnsureInProjectDirectory();

            var rootCommand = new RootCommand("Watari Framework");

            var devCommand = new Command("dev", "Run in development mode");
            devCommand.SetAction((parseResult) =>
            {
                new GenerateCommand(framework.Options).Execute();
                framework.Start(true);
            });

            var publishCommand = new Command("publish", "Publish the application");
            publishCommand.SetAction((parseResult) =>
            {
                new GenerateCommand(framework.Options).Execute();
                return new PublishCommand(framework.Options).ExecuteAsync();
            });

            var generateCommand = new Command("generate", "Generate TypeScript types");
            generateCommand.SetAction((parseResult) => new GenerateCommand(framework.Options).Execute());

            rootCommand.Add(devCommand);
            rootCommand.Add(publishCommand);
            rootCommand.Add(generateCommand);
            return rootCommand.Parse(args).Invoke() == 0;
        }
    }
}