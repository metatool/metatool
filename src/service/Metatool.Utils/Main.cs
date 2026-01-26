using Metatool.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Metatool.Utils;

public static class Main
{
	public static IServiceCollection AddMetatoolUtils(this IServiceCollection services) =>
		services.AddSingleton<IShell, Shell>()
			.AddSingleton<IWindowManager, WindowManager>()
			.AddSingleton<IVirtualDesktopManager, VirtualDesktopManager>()
            .AddSingleton<IClipboard, Clipboard>()
            .AddSingleton<IUiDispatcher,UiDispatcher>()
			.AddSingleton<IFileExplorer, FileExplorer>();
}