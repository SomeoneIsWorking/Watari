namespace Watari.Types;

public class TypeGeneratorOptions
{
    public required string OutputPath { get; set; }
    public required List<Type> ExposedTypes { get; set; }
    public required Dictionary<Type, object> Handlers { get; set; }
}