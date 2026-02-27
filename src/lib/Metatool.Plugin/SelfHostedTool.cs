namespace Metatool.Plugin;
public class SelfHostedTool
{
	public static T BuildTool<T, TConfig>(string configSection, Action<IServiceCollection> configureServices = null) where TConfig : class, new()
	{
		var config = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile("config.json", optional: false, reloadOnChange: true)
			.Build();

		var serviceProvider = new ServiceCollection()
			.AddLogging(builder =>
				builder
					.AddConfiguration(config.GetSection("Logging"))
					.AddConsole()
					.AddDebug()
			)
			.ConfigScreenHint()
			.ConfigMetatoolUtils()
			.AddSingleton<INotify, Notify>()
			.Configure<MetatoolConfig>(config)
			.AddSingleton(typeof(IConfig<>), typeof(ToolConfig<>))
			.Configure<TConfig>(config.GetSection("Tools").GetSection(configSection))
			.AddSingleton<IKeyboard, Keyboard>()
			.AddSingleton<IMouse, Mouse>()
			.AddSingleton<ICommandManager, CommandManager>()
			.BuildServiceProvider();
		Services.SetDefaultProvider(serviceProvider);

		return Services.Create<T>();
	}
}