using System;
using System.CommandLine;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Watari.Commands;

namespace Watari;

public class Framework(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;

    public bool Run(string[] args)
    {
        if (CliUtils.IsPublished())
        {
            return Start(false);
        }
        return CommandRunner.Run(this, args);
    }

    public bool Start(bool dev)
    {
        var services = Options.Services;
        services.AddSingleton<TypeConverter>();
        services.Configure<ServerOptions>(serverOptions =>
        {
            serverOptions.Dev = dev;
            serverOptions.ServerPort = Options.ServerPort;
            serverOptions.FrontendDistPath = CliUtils.JoinPath(Options.FrontendPath, "dist");
            serverOptions.ExposedTypes = Options.ExposedTypes;
        });
        Options.ConfigureServices?.Invoke(services);

        services.AddSingleton<Server>();
        var context = new WatariContext
        {
            Options = Options
        };
        services.AddSingleton(context);

        var serviceProvider = services.BuildServiceProvider();
        App.Start(dev, serviceProvider);
        return true;
    }

}
