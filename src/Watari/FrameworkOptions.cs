using Watari.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Watari;

public class FrameworkOptions
{
    public bool Dev { get; set; }
    public string FrontendPath { get; set; } = string.Empty;
    public int DevPort { get; set; } = 8983;
    public int ServerPort { get; set; } = 7533;
    public List<Type> ExposedTypes { get; } = [];
    public Action<IServiceCollection>? ConfigureServices { get; set; }
    public ServiceCollection Services { get; } = new();
}