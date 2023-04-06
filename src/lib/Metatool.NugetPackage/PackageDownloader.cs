using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Metatool.NugetPackage;

public class PackageDownloader
{
	private readonly ILogger _logger;

	private IList<SourceRepository> _sourceRepos;

	public PackageDownloader(Microsoft.Extensions.Logging.ILogger logger)
	{
		_logger = new NugetLogger(logger);
		NugetHelper.Instance.Logger = _logger;
	}

	public async Task<List<DllInfo>> DownloadPackage(PackageWrapper packageWrapper,string folder, bool disableCache = false, bool includePrelease = true, bool allowUnlisted = false)
	{
		_sourceRepos = new List<SourceRepository> {packageWrapper.sourceRepository};

		var packageIdentity = packageWrapper.rootPackageIdentity;
		var providers = new List<Lazy<INuGetResourceProvider>>();
		providers.AddRange(Repository.Provider.GetCoreV3());

		var rootPath = folder;
		var settings = Settings.LoadDefaultSettings(rootPath, null, new MachineWideSettings());
		var packageSourceProvider = new PackageSourceProvider(settings);
		var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, providers);
		var project = new NuGet.ProjectManagement.FolderNuGetProject(rootPath);
		var packageManager = new NuGet.PackageManagement.NuGetPackageManager(sourceRepositoryProvider, settings, rootPath)
		{
			PackagesFolderNuGetProject = project
		};

		var projectContext = new ProjectContext();
		var resolutionContext =
			new NuGet.PackageManagement.ResolutionContext(
				NuGet.Resolver.DependencyBehavior.Lowest,
				includePrelease,
				allowUnlisted,
				NuGet.PackageManagement.VersionConstraints.None);


		if (disableCache)
		{
			resolutionContext.SourceCacheContext.NoCache = true;
			resolutionContext.SourceCacheContext.DirectDownload = true;
		}

		var downloadContext = new PackageDownloadContext(resolutionContext.SourceCacheContext,
			rootPath, resolutionContext.SourceCacheContext.DirectDownload);

		var packageAlreadyExists = packageManager.PackageExistsInPackagesFolder(packageIdentity,
			NuGet.Packaging.PackageSaveMode.None);
		if (!packageAlreadyExists)
		{
			await packageManager.InstallPackageAsync(
				project,
				packageIdentity,
				resolutionContext,
				projectContext,
				downloadContext,
				_sourceRepos, 
				new List<SourceRepository>(),
				CancellationToken.None);

			var packageDeps=  packageManager.GetInstalledPackagesDependencyInfo(project, CancellationToken.None, true);
			_logger.LogInformation($"Package {packageIdentity.Id} is got Installed at  | {project.GetInstalledPath(packageIdentity)} ");
		}
		else
		{
			_logger.LogInformation($"Package {packageIdentity.Id} is Already Installed at  | {project.GetInstalledPath(packageIdentity)} " +
			                       $" | skipping installation !!");
		}
		var dlls = NugetHelper.Instance.GetInstallPackagesDllPath(packageWrapper, ref project);
		_logger.LogInformation($"done for package {packageIdentity.Id} , with total dlls {dlls.Count}");
		return dlls;
	}

}