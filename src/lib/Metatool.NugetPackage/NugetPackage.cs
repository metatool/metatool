﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Metatool.Service;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Credentials;
using NuGet.Frameworks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Metatool.NugetPackage;

internal class NugetPackage
{
	private readonly IEnumerable<string>        _configFilePaths;
	internal readonly List<PackageSource> _packageSources;
	internal readonly List<SourceRepository> _sourceRepositories;
	private readonly ExceptionDispatchInfo      _initializationException;
	private readonly ILogger                    _logger;
	private readonly NugetManager               _manager;

	public string GlobalPackageFolder { get; }

	public string PackageFolder => _manager.PackageFolder;

	public NugetPackage(ILogger logger, NugetManager manager)
	{
		_manager = manager;
		_logger  = logger;
		try
		{
			ISettings settings;
			try
			{
				settings = Settings.LoadDefaultSettings(
					root: null,
					configFileName: null,
					machineWideSettings: new XPlatMachineWideSetting());
			}
			catch (NuGetConfigurationException)
			{
				// create default settings using a non-existent config file
				settings = new Settings(nameof(NugetPackage));
			}

			GlobalPackageFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
			_configFilePaths    = new List<string>(); //SettingsUtility.GetConfigFilePaths(settings);
			var sources     = SettingsUtility.GetEnabledSources(settings);
			_packageSources     = _manager.AdditionalSources.Select(p=>new PackageSource(p.source,p.name)).Concat(sources).ToList();
			DefaultCredentialServiceUtility.SetupDefaultCredentialService(NullLogger.Instance,
				nonInteractive: false);

			var sourceProvider = new PackageSourceProvider(settings);
			var providers = new List<Lazy<INuGetResourceProvider>>();
			providers.AddRange(Repository.Provider.GetCoreV3());
			_sourceRepositories = _packageSources.Select(s => new SourceRepository(s, providers)).ToList();
		}
		catch (Exception e)
		{
			_logger?.LogError(e.Message + e.StackTrace);
			_initializationException = ExceptionDispatchInfo.Capture(e);
		}
	}

	internal RestoreParams CreateRestoreParams()
	{
		_initializationException?.Throw();
		var restoreParams = new RestoreParams();
		foreach (var packageSource in _packageSources)
		{
			restoreParams.Sources.Add(packageSource);
		}

		foreach (var configFile in _configFilePaths)
		{
			restoreParams.ConfigFilePaths.Add(configFile);
		}

		restoreParams.PackagesPath = PackageFolder;
		return restoreParams;
	}

	internal (IList<string> compile, IList<string> runtime, IList<string> analyzers) ParseLockFile(
		string lockFilePath, CancellationToken cancellationToken,
		NuGetFramework targetFramework, string frameworkVersion, HashSet<LibraryRef> libraries)
	{
		JObject obj;
		using (var reader = File.OpenText(lockFilePath))
		{
			obj = PackageUtils.LoadJson(reader);
		}

		var (compile, runtime, analyzers) = PackageUtils.ReadProjectLockJson(obj,
			PackageFolder,
			targetFramework.DotNetFrameworkName);

		TransformLockFileToDepsFile(obj, targetFramework.DotNetFrameworkName, libraries);

		cancellationToken.ThrowIfCancellationRequested();

		using (var writer = new JsonTextWriter(File.CreateText(lockFilePath)) {Formatting = Formatting.Indented})
		{
			obj.WriteTo(writer);
		}

		cancellationToken.ThrowIfCancellationRequested();
		return (compile, runtime, analyzers);
	}

	private void TransformLockFileToDepsFile(JObject obj, string targetFramework, HashSet<LibraryRef> _libraries)
	{
		foreach (var p in obj.Properties().Where(p => p.Name != "targets" && p.Name != "libraries").ToArray())
		{
			p.Remove();
		}

		obj.AddFirst(new JProperty("runtimeTarget", new JObject(new JProperty("name", targetFramework))));

		var libraries = (JObject) obj["libraries"];

		foreach (var fx in ((JObject) obj["targets"]).Properties())
		{
			foreach (var p in fx.Value.Children<JProperty>().Where(IsRuntimeEmptyOrPlaceholder).ToArray())
			{
				p.Remove();
				libraries.Remove(p.Name);
			}

			foreach (var library in _libraries)
			{
				if (library.Path != null)
				{
					((JObject) fx.Value).Add(new JProperty(library.AssemblyName + "/0.0.0", new JObject(
						new JProperty("type", "project"),
						new JProperty("runtime", new JObject(
							new JProperty(library.AssemblyName + ".dll", new JObject()))))));
				}
			}
		}

		foreach (var p in libraries.Properties())
		{
			p.Value["serviceable"] = true;
			((JObject) p.Value).Remove("files");
		}

		foreach (var library in _libraries)
		{
			if (library.Path != null)
			{
				libraries.Add(new JProperty(library.AssemblyName + "/0.0.0", new JObject(
					new JProperty("type", "project"),
					new JProperty("serviceable", false),
					new JProperty("sha512", ""))));
			}
		}

		bool IsRuntimeEmptyOrPlaceholder(JProperty p)
		{
			return !(p.Value["runtime"] is JObject runtime) ||
			       runtime.Properties().All(pp => PackageUtils.IsPlaceholder(pp.Name));
		}
	}
}