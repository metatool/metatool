using System.IO;
using System.Linq;
using Metatool.Plugin;
using Microsoft.Extensions.Configuration;

namespace Metatool.Plugins;

public static class PluginConfigExt
{
	public static IConfigurationBuilder AddPluginsConfig(this IConfigurationBuilder builder)
	{
		var toolsDirs = PluginManager.GetToolDirectories();

		toolsDirs.ToList().ForEach(toolDir =>
		{
			builder.AddJsonFile(Path.Combine(toolDir, "config.json"), optional: true,
				reloadOnChange: true);

		}) ;
		return builder;
	}
}