namespace Watari;

public class WatariContext
{
    public Controls.Platform.Application Application { get; set; } = null!;
    public Controls.Platform.Window MainWindow { get; set; } = null!;
    public Controls.Platform.WebView WebView { get; set; } = null!;
    public required FrameworkOptions Options { get; set; }
    public Server Server { get; set; } = null!;
    public string BasePath { get; } = CliUtils.GetProjectPath();
    public string PathCombine(params string[] paths)
    {
        return Path.Combine(BasePath, Path.Combine(paths));
    }
}