
namespace Watari;

public interface ITypeHandler
{
    object? ToTypeScript(object? val);
    object? FromTypeScript(object? val);
}

public interface ITypeHandler<T, TS> : ITypeHandler
{
    TS ToTypeScript(T val);
    T FromTypeScript(TS val);
}
