using System.Text.Json;
using System.Text.Json.Serialization;

namespace Watari;

public class TypeConverter(ICollection<JsonConverter> typeHandlers)
{
    private JsonSerializerOptions? _jsonOptions;
    public JsonSerializerOptions JsonOptions => _jsonOptions ??= BuildSerializerOptions();

    private JsonSerializerOptions BuildSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        foreach (var converter in typeHandlers)
        {
            options.Converters.Add(converter);
        }
        return options;
    }

    public object? ParseInput(string json, Type type)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        return ResolveInput(type, jsonElement);
    }

    public string SerializeOutput(object? value)
    {
        var type = value?.GetType() ?? typeof(void);
        var resolved = ResolveResponse(type, value);
        return JsonSerializer.Serialize(resolved, JsonOptions);
    }

    public object? ResolveResponse(Type type, object? value)
    {
        if (value == null) return null;

        if (type == typeof(void))
        {
            return null; // Though void methods return NoContent earlier
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var task = (Task)value;
            var resultProperty = task.GetType().GetProperty("Result");
            var actualResult = resultProperty?.GetValue(task);
            var innerType = type.GetGenericArguments()[0];
            return ResolveResponse(innerType, actualResult);
        }
        else if (type == typeof(Task))
        {
            var task = (Task)value;
            return null; // Indicates NoContent
        }
        else
        {
            return value;
        }
    }

    public object? ResolveInput(Type type, JsonElement json)
    {
        return JsonSerializer.Deserialize(json.GetRawText(), type, JsonOptions);
    }
}
