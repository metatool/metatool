using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool.HostService;

public class StartupService : IHostedService
{
	private readonly IConfiguration _config;
	private readonly ILogger _logger;
	

	public StartupService(IConfiguration config, ILogger<StartupService> logger)
	{
		_config = config;
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
		args = args.Where(i => i != Program.AdminFlagLong).ToArray();
		return new ArgumentProcessor(args).Run();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

}