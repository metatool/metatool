using Metatool.ScreenHint.HintUI;
using Metatool.ScreenPoint;
using Metatool.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Metatool.ScreenHint;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection ConfigScreenHint(this IServiceCollection services) =>
		services
			.AddSingleton<IUIElementsDetector, UIElementsDetector>()
			.AddSingleton<IHintsBuilder, HintsBuilder>()
			.AddSingleton<IHintUI, HintUI.HintUI>()
			.AddSingleton<IScreenHint, ScreenHint>();
}
