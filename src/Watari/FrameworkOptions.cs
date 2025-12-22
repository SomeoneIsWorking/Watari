using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Watari;

public class FrameworkOptions
{
    public string FrontendPath { get; set; } = string.Empty;
    public int DevPort { get; set; } = 8983;
    public int ServerPort { get; set; } = 7533;
    public List<Type> ExposedTypes { get; } = [];
    public ICollection<JsonConverter> JsonConverters { get; } = [];
    public Dictionary<Type, object> TypeHandlers { get; } = [];
    public ServiceCollection Services { get; } = new();
}