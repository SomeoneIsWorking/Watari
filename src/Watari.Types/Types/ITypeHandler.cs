namespace Watari.Types;

public interface ITypeHandler<T, TS>
{
    TS ToTypeScript(T val);
    T FromTypeScript(TS val);
}
