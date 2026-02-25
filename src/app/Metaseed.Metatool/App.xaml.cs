using System;
using System.Threading;
using System.Windows;
using Metatool.Core;
using Metatool.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool;

public partial class App(IConfig<MetatoolConfig> config, IHostEnvironment hostEnv, ILogger<App> logger, INotify notify)
    : Application
{
    private void ConfigNotify(Scaffold scaffolder)
	{
		notify.AddContextMenuItem("Show Log", e =>
		{
			if (e.IsChecked)
				ConsoleExt.ShowConsole();
			else
				ConsoleExt.HideConsole();
		}, null, true, hostEnv.IsDevelopment());

		notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
			AutoStartManager.IsAutoStart);
		notify.AddContextMenuItem("Register", e => { scaffolder.Register(); });

		notify.ShowMessage("Metatool started!");
	}


	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		Current.MainWindow = new MainWindow();
		notify.ShowMessage("Metatool starting...");

		var scaffold = new Scaffold(logger);
		scaffold.CommonSetup(config);

		ConfigNotify(scaffold);
		if(Environment.GetEnvironmentVariable("MetatoolDir") == null) {
			scaffold.Register();
		}
		logger.LogInformation($"Registered MetatoolDir: {Environment.GetEnvironmentVariable("MetatoolDir")}");
		logger.LogInformation("Metatool started!");
	}

	internal static void RunApp(Action action = null)
	{
		var newWindowThread = new Thread(()=>Start(action)){Name = "UI Thread"};
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