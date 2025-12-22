using Microsoft.Extensions.DependencyInjection;
using Watari.Types;

namespace Watari.Commands;

public class GenerateCommand(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;

    public void Execute()
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
    }
}