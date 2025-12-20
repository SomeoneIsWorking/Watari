
using Microsoft.AspNetCore.Builder;

namespace Watari;

public class Server
{
    public NpmManager NpmManager { get; } = new NpmManager();
    public WebApplication WebApplication { get; private set; } = null!;

    public async Task Start(ServerOptions options)
    {
        if (options.Dev)
        {
            await NpmManager.StartDev(options.FrontendPath, options.DevPort, options.CancellationToken);
        }
        else
        {
            WebApplication webApplication = WebApplication.CreateBuilder().Build();
            await webApplication.StartAsync(options.CancellationToken);
        }
    }

    public Server Stop(ServerOptions options)
    {
        _ = Start(options);
        return this;
    }
}
