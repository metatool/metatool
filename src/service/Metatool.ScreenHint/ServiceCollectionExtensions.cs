using Metatool.ScreenHint.HintUI;
using Metatool.ScreenPoint;
using Metatool.Service;
using Metatool.Service.ScreenHint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Metatool.ScreenHint;

public static class ServiceCollectionExtensions
{
	static bool _registered;
	public static IServiceCollection ConfigScreenHint(this IServiceCollection services)
	{
		if (_registered) return services;
		_registered = true;
		var conf = Services.Get<IConfiguration>();
		var screenHintConfig = conf.GetSection("Services:ScreenHintConfig");
		var hintEncoder = screenHintConfig.GetSection("HintEncoder");
		return services
		// to use IConfig<ScreenHintConfig>
			.Configure<ScreenHintConfig>(screenHintConfig)
			// to use IConfig<HintEncoderConfig>
			.Configure<HintEncoderConfig>(hintEncoder)
			.AddSingleton<IHintsBuilder, HintsBuilderNew>()
			.AddSingleton<IHintUI, HintUI.HintUI>()
			.AddSingleton<IScreenHint, ScreenHint>();
	}
}
