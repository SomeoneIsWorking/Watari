using Microsoft.Extensions.DependencyInjection;

namespace Watari.Commands;

public class DevCommand(FrameworkOptions options)
{
    public FrameworkOptions Options { get; } = options;


}