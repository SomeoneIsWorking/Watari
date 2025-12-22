using System;
using Microsoft.Extensions.DependencyInjection;
using Watari.Types;

namespace Watari.Commands;

public class GenerateCommand(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;

    public void Execute()
    {
        var services = Options.Services;
        services.AddSingleton<TypeConverter>();
        Options.ConfigureServices?.Invoke(services);

        var provider = services.BuildServiceProvider();
        var success = new TypeGenerator(new TypeGeneratorOptions
        {
            OutputPath = Options.FrontendPath,
            ExposedTypes = Options.ExposedTypes,
            Provider = provider
        }).Generate();

        if (!success)
        {
            throw new Exception("Type generation failed");
        }
    }
}