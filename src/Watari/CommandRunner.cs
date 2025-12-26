using System.CommandLine;
using Watari.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Watari
{
    public class CommandRunner
    {
        public static bool Run(Framework framework, string[] args)
        {
            CliUtils.EnsureInProjectDirectory();

            // Build a temporary service provider for logging
            IServiceCollection services = new ServiceCollection();
            foreach (var service in framework.Options.Services)
            {
                services.Add(service);
            }
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var loggerGen = loggerFactory.CreateLogger<GenerateCommand>();
            var loggerPub = loggerFactory.CreateLogger<PublishCommand>();

            var rootCommand = new RootCommand("Watari Framework");

            var devCommand = new Command("dev", "Run in development mode");
            devCommand.SetAction((parseResult) =>
            {
                new GenerateCommand(framework.Options, loggerGen).Execute();
                framework.Start(true);
            });

            var publishCommand = new Command("publish", "Publish the application");
            publishCommand.SetAction((parseResult) =>
            {
                new GenerateCommand(framework.Options, loggerGen).Execute();
                return new PublishCommand(framework.Options, loggerPub).ExecuteAsync();
            });

            var generateCommand = new Command("generate", "Generate TypeScript types");
            generateCommand.SetAction((parseResult) => new GenerateCommand(framework.Options, loggerGen).Execute());

            rootCommand.Add(devCommand);
            rootCommand.Add(publishCommand);
            rootCommand.Add(generateCommand);
            return rootCommand.Parse(args).Invoke() == 0;
        }
    }
}