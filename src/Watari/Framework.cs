using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton(new TypeConverter(Options.JsonConverters));
        services.Configure<ServerOptions>(serverOptions =>
        {
            serverOptions.Dev = dev;
            serverOptions.ServerPort = Options.ServerPort;
            serverOptions.ExposedTypes = Options.ExposedTypes;
        });

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
