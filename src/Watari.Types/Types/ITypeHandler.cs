namespace Watari.Types;

public interface ITypeHandler
{
}

public interface ITypeHandler<T> : ITypeHandler
{
}

public interface ITypeHandler<T, TS> : ITypeHandler<T>
{
    TS ToTypeScript(T val);
    T FromTypeScript(TS val);
}
