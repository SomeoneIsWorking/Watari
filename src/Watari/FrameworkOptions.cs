using System.Runtime.CompilerServices;

namespace Watari;

public class FrameworkOptions
{
    public bool Dev { get; set; }
    public required string FrontendPath { get; set; }
    public int DevPort { get; set; } = 8983;

    public static string GetCallingFilePath([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}