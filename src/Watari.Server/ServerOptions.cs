namespace Watari;

public class ServerOptions
{
    public bool Dev { get; set; }
    public int DevPort { get; set; }
    public required string FrontendPath { get; set; }
    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
}