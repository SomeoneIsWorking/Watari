namespace Watari;

public static class WatariResources
{
    private static readonly Lazy<string> _watariJs = new Lazy<string>(() => GetFromResource("Watari.watari.js"));
    private static readonly Lazy<string> _watariDts = new Lazy<string>(() => GetFromResource("Watari.watari.d.ts"));

    public static string WatariJs => _watariJs.Value;
    public static string WatariDts => _watariDts.Value;

    private static string GetFromResource(string resourceName)
    {
        var assembly = typeof(WatariResources).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"{resourceName} not found in embedded resources.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}