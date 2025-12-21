
namespace Watari;

public interface ITypeHandler
{
}

public interface ITypeHandler<T, TS> : ITypeHandler
{
    TS ToTypeScript(T val);
    T FromTypeScript(TS val);
}
