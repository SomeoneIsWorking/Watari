using System.Reflection;

namespace Watari;

public static class CliUtils
{
    private static readonly Assembly? EntryAssembly = Assembly.GetEntryAssembly();
    public static string GetProjectPath()
    {
        var assemblyLocation = EntryAssembly?.Location;
        if (string.IsNullOrEmpty(assemblyLocation))
        {
            throw new InvalidOperationException("Unable to determine assembly location.");
        }
        string executableDir = Path.GetDirectoryName(assemblyLocation)!;
        string publishedMarker = Path.Combine(executableDir, ".published");
        if (File.Exists(publishedMarker))
        {
            return executableDir;
        }
        EnsureInProjectDirectory();
        return Directory.GetCurrentDirectory();
    }

    public static string JoinPath(params string[] path)
    {
        return Path.Combine(GetProjectPath(), Path.Combine(path));
    }

    public static void EnsureInProjectDirectory()
    {
        // Otherwise, assume development mode, check for csproj in current dir
        var entryAssembly = EntryAssembly
            ?? throw new InvalidOperationException("Unable to determine entry assembly.");
        string assemblyName = entryAssembly.GetName().Name!;
        string csprojPath = Path.Combine(Directory.GetCurrentDirectory(), $"{assemblyName}.csproj");

        if (!File.Exists(csprojPath))
        {
            throw new InvalidOperationException("Please run from project directory");
        }
    }

    public static bool IsPublished()
    {
        if (string.IsNullOrEmpty(EntryAssembly?.Location))
        {
            return false;
        }
        string executableDir = Path.GetDirectoryName(EntryAssembly.Location)!;
        string publishedMarker = Path.Combine(executableDir, ".published");
        return File.Exists(publishedMarker);
    }
}
