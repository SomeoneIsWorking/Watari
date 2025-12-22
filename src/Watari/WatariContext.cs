namespace Watari;

public class WatariContext
{
    public Controls.Platform.Application Application { get; set; } = null!;
    public Controls.Platform.Window MainWindow { get; set; } = null!;
    public Controls.Platform.WebView WebView { get; set; } = null!;
    public required FrameworkOptions Options { get; set; }
}