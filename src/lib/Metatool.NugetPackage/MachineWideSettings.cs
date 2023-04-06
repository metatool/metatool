using System;
using System.Collections.Generic;
using NuGet.Common;
using NuGet.Configuration;

namespace Metatool.NugetPackage;

public class MachineWideSettings : IMachineWideSettings
{
	private readonly Lazy<ISettings> _settings;

	public MachineWideSettings()
	{
		var baseDirectory = NuGetEnvironment.GetFolderPath(NuGetFolderPath.MachineWideConfigDirectory);
		_settings = new Lazy<ISettings>(
			() => global::NuGet.Configuration.Settings.LoadMachineWideSettings(baseDirectory));
	}

	public ISettings Settings => _settings.Value;

}