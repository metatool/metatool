using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool.HostService;

public class StartupService(IConfiguration config, ILogger<StartupService> logger, [FromKeyedServices("args")] string[] args) : IHostedService
{

    public Task StartAsync(CancellationToken cancellationToken)
    {
        //var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        //args = args.Where(i => i != Program.AdminFlagLong).ToArray();
        return new ArgumentProcessor(args).Run();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

}