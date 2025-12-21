using System.Text.Json;
using System.Text.Json.Serialization;

namespace Watari.Types;

public class TypeHandlerConverter<T, U>(ITypeHandler<T, U> handler) : JsonConverter<T>
{
    private readonly ITypeHandler<T, U> _handler = handler;

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