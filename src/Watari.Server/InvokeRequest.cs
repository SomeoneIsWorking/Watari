using System.Text.Json;
using System.Text.Json.Serialization;

namespace Watari;

public class InvokeRequest
{
    [JsonPropertyName("method")]
    public required string Method { get; set; }
    [JsonPropertyName("args")]
    public required List<JsonElement> Args { get; set; }
}