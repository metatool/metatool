﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Service;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Protocol.Core.Types;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Metatool.NugetPackage;

public partial class NugetManager
{
	private readonly ILogger _logger;
	private readonly SemaphoreSlim _restoreLock = new(1, 1);
	private readonly HashSet<LibraryRef> _libraries = new();
	private NuGetFramework _targetFramework;
	private CancellationTokenSource _restoreCts;
	private string _frameworkVersion;

	public ImmutableArray<string> LocalLibraryPaths { get; private set; } = ImmutableArray<string>.Empty;
	public event Action<RestoreSuccessResult> RestoreSuccess;

	public string Id { get; set; } = Guid.NewGuid().ToString();

	public string RestorePath { get; set; }

	public string PackageFolder { get; set; } = Context.PackageDirectory;

	public List<(string name, string source)> AdditionalSources { get; set; } =
		new()
		{
			("metatool.pkg.source",Context.PackageSourceDirectory)
			// ("metatool.pkg.used", Context.PackageDirectory)
		};

	public List<PackageSource> PackageSources => _nugetPackage._packageSources;
	public List<SourceRepository> SourceRepositories => _nugetPackage._sourceRepositories;

	public bool IsRestoring { get; private set; }
	public bool RestoreFailed { get; private set; }
	private readonly NugetPackage _nugetPackage;

	public event Action<IReadOnlyList<string>> RestoreError;

	public NugetManager(ILogger logger)
	{
		_logger = logger;
		_packageDownloader = new PackageDownloader(logger);
		var framework = Assembly
			.GetEntryAssembly()?
			.GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
		var str = framework?.Split(",Version=v");
		Debug.Assert(str != null, nameof(str) + " != null");
		var frameworkName = string.Concat(str);
		_targetFramework = NuGetFramework.ParseFolder(frameworkName);
		_frameworkVersion = str?[1];
		_nugetPackage = new NugetPackage(_logger, this);
	}

	public void SetTargetFramework(string targetFrameworkMoniker, string frameworkVersion = null)
	{
		_targetFramework = NuGetFramework.ParseFolder(targetFrameworkMoniker);
		_frameworkVersion = frameworkVersion;
		RefreshPackages();
	}
	private bool CheckChanged(IReadOnlyList<LibraryRef> libraries)
	{
		var changed = false;

		if (_libraries.Count > 0 && (libraries == null || libraries.Count == 0))
		{
			_libraries.Clear();
			LocalLibraryPaths = ImmutableArray<string>.Empty;
			changed = true;
		}
		else
		{
			var removed = _libraries.RemoveWhere(p =>
			{
				var remove = !libraries.Contains(p);
				if (remove && p.Path != null)
				{
					LocalLibraryPaths = LocalLibraryPaths.Remove(p.Path);
				}

				return remove;
			});

			if (removed > 0)
			{
				changed = true;
			}

			foreach (var library in libraries)
			{
				if (_libraries.Add(library))
				{
					if (library.Path != null)
					{
						LocalLibraryPaths = LocalLibraryPaths.Add(library.Path);
					}

					changed = true;
				}
			}
		}
		return changed;
	}

	public void Restore(IReadOnlyList<LibraryRef> libraries)
	{
		if (CheckChanged(libraries))
		{
			RefreshPackages().GetAwaiter().GetResult();
		}
	}
	public async Task<RestoreResult> RestoreAsync(IReadOnlyList<LibraryRef> libraries)
	{
		if (CheckChanged(libraries))
			return await RefreshPackages();
		return new RestoreResult(new []{"no change in libaries, so no need to restore Nuget packages"}, true, true);
	}

	private async Task<RestoreResult> RefreshPackages()
	{
		if (RestorePath == null || _targetFramework == null)
			return new RestoreResult(
				new[] { "RestorePath is null or _taggetFramework is null" }, false, true);

		_restoreCts?.Cancel();

		var packages = _libraries?.ToArray();

		var restoreCts = new CancellationTokenSource();
		var cancellationToken = restoreCts.Token;
		_restoreCts = restoreCts;

		return await RefreshPackagesAsync(packages, cancellationToken);
	}

	public async Task<RestoreResult> RefreshPackagesAsync(LibraryRef[] libraries, CancellationToken cancellationToken,
		IList<PackageSource> sources = null)
	{
		await _restoreLock.WaitAsync(cancellationToken).ConfigureAwait(false);
		IsRestoring = true;
		try
		{
			var restoreParams = _nugetPackage.CreateRestoreParams();
			restoreParams.ProjectName = Id;
			restoreParams.OutputPath = RestorePath;
			restoreParams.Libraries = libraries;
			restoreParams.TargetFramework = _targetFramework;
			restoreParams.FrameworkVersion = _frameworkVersion;
			if (sources != null) restoreParams.Sources = sources;

			var lockFilePath = Path.Combine(RestorePath, "project.assets.json");

			if (File.Exists(lockFilePath)) File.Delete(lockFilePath);

			var result = await PackageUtils.RestoreAsync(restoreParams, NullLogger.Instance, cancellationToken)
				.ConfigureAwait(false);

			if (!result.Success)
			{
				_logger.LogWarning("Package Restore Error!");
				RestoreFailed = true;
				RestoreError?.Invoke(result.Errors);
				return result;
			}

			RestoreFailed = false;

			if (result.NoOp)
			{
				_logger.LogInformation("No operation taken for lib restore.");
				return result;
			}

			cancellationToken.ThrowIfCancellationRequested();

			var (compile, runtime, analyzers) = _nugetPackage.ParseLockFile(lockFilePath, cancellationToken,
				_targetFramework, _frameworkVersion,
				_libraries);
			var r = new RestoreSuccessResult(compile, runtime, analyzers);
			RestoreSuccess?.Invoke(r);
			result.SuccessResult = r;
			return result;
		}
		catch (Exception e) when (!(e is OperationCanceledException))
		{
			_logger?.LogError(e.Message + e.StackTrace);
			return new RestoreResult(new List<string>() { e.Message, e.StackTrace }, false, false);
		}
		finally
		{
			_restoreLock.Release();
			IsRestoring = false;
		}
	}
}