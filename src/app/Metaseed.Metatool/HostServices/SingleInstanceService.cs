using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool.HostService;

public class SingleInstanceService : IHostedService
{
	private Mutex _mutex;
	public SingleInstanceService(IConfig<MetatoolConfig> config, ILogger<StartupService> logger)
	{
		_mutex = new Mutex(true, "Metatool", out var created);

		if (config.CurrentValue.SingleInstance && !created)
		{
			//MessageBox.Show("Metatool is already running!");
			var currentProcess = Process.GetCurrentProcess();
			Process.GetProcessesByName(currentProcess.ProcessName)
				.Where(p => p.Id != currentProcess.Id)
				.ToList().ForEach(p => p.Kill());
			logger.LogCritical("Metatool is already running! kill it");

			//Process.GetCurrentProcess().Kill();
		}
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

}