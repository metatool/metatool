using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool.HostService;

internal class ConsoleInputService : IHostedService
{
	public ConsoleInputService(ILogger<ConsoleInputService> logger)
	{
		_logger = logger;
	}
	private static string Reply(string message)
	{
		static void ClearCurrentConsoleLine()
		{
			int currentLineCursor = Console.CursorTop;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, currentLineCursor);
		}
		ClearCurrentConsoleLine();
		Console.WriteLine(message);
		return message;
	}

	public static string HelpInfo = $@"
****************************************
Metatool (v{typeof(ConsoleInputService).Assembly.GetName().Version}) Help Info
****************************************
h: display help info;
c: clear console;
****************************************";
	private ILogger<ConsoleInputService> _logger;

	public void Process(ConsoleKeyInfo key)
	{
		switch (key.KeyChar)
		{
			case 'h': Reply(HelpInfo); break;
			case 'c': Console.Clear(); break;
			default: _logger.LogInformation($"you typed option:{ key.KeyChar}" ); break;
		}
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		Task.Run(() =>
		{
			_logger.LogInformation(ConsoleInputService.HelpInfo);
			while (true)
			{
				Process(Console.ReadKey());
			}
		});
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}