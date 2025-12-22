namespace Watari;

public class ServerOptions
{
    public bool Dev { get; set; }
    public int ServerPort { get; set; } = 5000;
    public required string FrontendDistPath { get; set; }
    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    public required List<Type> ExposedTypes { get; set; }
}