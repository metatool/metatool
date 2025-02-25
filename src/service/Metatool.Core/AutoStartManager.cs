using System;
using System.Reflection;
using Metatool.Service;
using Microsoft.Win32;

namespace Metatool.Core;

public static class AutoStartManager
{
	static readonly RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
		(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

	static readonly string name      = Assembly.GetEntryAssembly().GetName().Name;
	static readonly string path      = $"{Context.AppDirectory}\\{name}.exe --admin";

	public static bool IsAutoStart
	{
		get => registryKey.GetValue(name) != null;
		set
		{
			if (value) registryKey.SetValue(name, path);
			else registryKey.DeleteValue(name);
		}
	}

}