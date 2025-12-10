using System;
using System.Threading;
using System.Windows;
using Metatool.Core;
using Metatool.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool;

public partial class App : Application
{
	private readonly IConfig<MetatoolConfig> _config;
	private readonly ILogger<App>            _logger;
	private readonly IHostEnvironment        _hostEnv;
	private          INotify                 _notify;

	public App(IConfig<MetatoolConfig> config, IHostEnvironment hostEnv, ILogger<App> logger, INotify notify)
	{
		_config  = config;
		_logger  = logger;
		_hostEnv = hostEnv;
		_notify  = notify;
	}

	private void ConfigNotify(Scaffold scaffolder)
	{
		_notify.AddContextMenuItem("Show Log", e =>
		{
			if (e.IsChecked)
				ConsoleExt.ShowConsole();
			else
				ConsoleExt.HideConsole();
		}, null, true, _hostEnv.IsDevelopment());

		_notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
			AutoStartManager.IsAutoStart);
		_notify.AddContextMenuItem("Register", e => { scaffolder.Register(); });

		_notify.ShowMessage("Metatool started!");
	}


	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		Current.MainWindow = new MainWindow();
		_notify.ShowMessage("Metatool starting...");

		var scaffold = new Scaffold(_logger);
		scaffold.CommonSetup(_config);

		ConfigNotify(scaffold);
		if(Environment.GetEnvironmentVariable("MetatoolDir") == null) {
			scaffold.Register();
		}
		_logger.LogInformation($"Registered MetatoolDir: {Environment.GetEnvironmentVariable("MetatoolDir")}");
		_logger.LogInformation("Metatool started!");
	}

	internal static void RunApp(Action action = null)
	{
		var newWindowThread = new Thread(()=>Start(action));
		newWindowThread.SetApartmentState(ApartmentState.STA);
		newWindowThread.IsBackground = true;
		newWindowThread.Start();
	}

	private static void Start(Action action = null)
	{
		var application = Services.Create<App>();
		Context.Dispatcher = application.Dispatcher;
        action?.Invoke();
         
		application.InitializeComponent();
		application.Run();
	}
}