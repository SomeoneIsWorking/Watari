using Watari.Types;
using Microsoft.Extensions.Logging;

namespace Watari.Commands;

public class GenerateCommand(FrameworkOptions options, ILogger<GenerateCommand> logger)
{
    public FrameworkOptions Options { get; } = options;

    public void Execute()
    {
        TypeGenerator.Generate(new TypeGeneratorOptions
        {
            OutputPath = Options.FrontendPath,
            ExposedTypes = Options.ExposedTypes,
            Handlers = Options.TypeHandlers,
            WatariDtsContent = WatariResources.WatariDts,
        }, logger);
    }
}