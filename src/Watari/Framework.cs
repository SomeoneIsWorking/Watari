using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
        var server = serviceProvider.GetRequiredService<Server>();

        // Automatically subscribe to events on exposed types and emit them
        AddListeners(serviceProvider, server);

        App.Start(dev, serviceProvider);
        return true;
    }

    private void AddListeners(ServiceProvider serviceProvider, Server server)
    {
        foreach (var type in Options.ExposedTypes)
        {
            var instance = serviceProvider.GetRequiredService(type);
            var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance);
            foreach (var evt in events)
            {
                var handlerType = evt.EventHandlerType;
                if (handlerType!.IsGenericType && handlerType.GetGenericTypeDefinition() == typeof(Action<>))
                {
                    var argType = handlerType.GetGenericArguments()[0];
                    var createHandlerMethod = typeof(Framework).GetMethod(nameof(CreateHandler), BindingFlags.NonPublic | BindingFlags.Static);
                    var genericMethod = createHandlerMethod!.MakeGenericMethod(argType);
                    var handler = genericMethod.Invoke(null, [server, type.Name, evt.Name]);
                    evt.AddEventHandler(instance, (Delegate)handler!);
                }
            }
        }
    }

    private static Action<T> CreateHandler<T>(Server server, string typeName, string eventName)
    {
        return async data => await server.EmitEvent($"{typeName}.{eventName}", data!);
    }
}
