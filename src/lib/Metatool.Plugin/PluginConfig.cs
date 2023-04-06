using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Metatool.Metatool.Plugin;

public class PluginConfig
{
	public PluginConfig(string mainAssemblyPath)
	{
		if (string.IsNullOrEmpty(mainAssemblyPath))
		{
			throw new ArgumentException("Value must be null or not empty", nameof(mainAssemblyPath));
		}

		if (!Path.IsPathRooted(mainAssemblyPath))
		{
			throw new ArgumentException("Value must be an absolute file path", nameof(mainAssemblyPath));
		}

		MainAssemblyPath = mainAssemblyPath;
	}

	public string MainAssemblyPath { get; }

	public ICollection<AssemblyName> PrivateAssemblies { get; set; } = new List<AssemblyName>();

	public ICollection<AssemblyName> SharedAssemblies { get; set; } = new List<AssemblyName>();
	public ICollection<string> SharedAssemblyPrefixes { get; set; } = new List<string>();

	public bool PreferSharedTypes { get; set; }

	/// <summary>
	/// The plugin can be unloaded from memory.
	/// </summary>
	public bool IsUnloadable { get; set; }
}