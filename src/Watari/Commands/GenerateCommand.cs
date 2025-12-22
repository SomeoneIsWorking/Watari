using Watari.Types;

namespace Watari.Commands;

public class GenerateCommand(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;

    public void Execute()
    {
        TypeGenerator.Generate(new TypeGeneratorOptions
        {
            OutputPath = Options.FrontendPath,
            ExposedTypes = Options.ExposedTypes,
            Handlers = Options.TypeHandlers,
        });
    }
}