
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Watari;

public class TypeHandlerConverter<T, U> : JsonConverter<T>
{
    private readonly ITypeHandler<T, U> _handler;

    public TypeHandlerConverter(ITypeHandler<T, U> handler)
    {
        _handler = handler;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dto = JsonSerializer.Deserialize<U>(ref reader, options);
        if (dto == null) return default;
        return _handler.FromTypeScript(dto);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var dto = _handler.ToTypeScript(value);
        JsonSerializer.Serialize(writer, dto, options);
    }
}