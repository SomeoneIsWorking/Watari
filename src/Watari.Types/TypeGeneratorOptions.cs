
namespace Watari;

public interface ITypeHandler<T, TS>
{
    TS ToTypeScript(T val);
    T FromTypeScript(TS val);
}

public class TypeGeneratorOptions
{
    public required string OutputPath { get; set; }
    public required List<Type> ExposedTypes { get; set; }
    public required Dictionary<Type, dynamic> Handlers { get; set; }
}