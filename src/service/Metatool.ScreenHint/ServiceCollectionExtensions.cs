using Metatool.ScreenHint.HintUI;
using Metatool.ScreenPoint;
using Metatool.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Metatool.ScreenHint;

public static class ServiceCollectionExtensions
{
	static bool _registered;
	public static IServiceCollection ConfigScreenHint(this IServiceCollection services)
	{
		if (_registered) return services;

		_registered = true;
		return services
			.AddSingleton<IUIElementsDetector, UIElementsDetector>()
			.AddSingleton<IHintsBuilder, HintsBuilder>()
			.AddSingleton<IHintUI, HintUI.HintUI>()
			.AddSingleton<IScreenHint, ScreenHint>();
	}
}
