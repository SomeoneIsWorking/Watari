
using Microsoft.AspNetCore.Builder;

namespace Watari;

public class Server
{
    public NpmManager NpmManager { get; } = new NpmManager();

    public Task Start(bool dev, string frontendPath)
    {
        if (dev)
        {
            NpmManager.StartDev(frontendPath);
        }

        return WebApplication.CreateBuilder().Build().RunAsync();
    }
}
