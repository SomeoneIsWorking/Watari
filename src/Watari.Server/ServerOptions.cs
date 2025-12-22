namespace Watari;

public class ServerOptions
{
    public required bool Dev { get; set; }
    public required int ServerPort { get; set; } = 5000;
    public CancellationToken CancellationToken { get; set; }
    public required List<Type> ExposedTypes { get; set; }
}