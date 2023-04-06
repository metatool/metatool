using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Metatool.NugetPackage;

public sealed class NugetHelper
{
	private static readonly Lazy<NugetHelper> lazy = new(() => new NugetHelper());

	public static NugetHelper Instance => lazy.Value;

	internal  ILogger Logger;

	public IEnumerable<SourceRepository> GetSourceRepos(IList<NugetRepository> repositories)
	{
		var providers = new List<Lazy<INuGetResourceProvider>>();
		providers.AddRange(Repository.Provider.GetCoreV3());

		IList<SourceRepository> sourceRepositories = new List<SourceRepository>();

		foreach (var repository in repositories.OrderBy(x => x.Order))
		{
			var packageSource = new PackageSource(repository.Source, repository.Name);
			if (repository.IsPrivate)
			{
				packageSource.Credentials = new PackageSourceCredential(repository.Name,
					repository.Username, repository.Password, repository.IsPasswordClearText, null);
			}

			var sourceRepository = new SourceRepository(packageSource, providers);
			sourceRepositories.Add(sourceRepository);
		}

		return sourceRepositories;
	}

	public SourceRepository GetSourceRepository(NugetRepository repository)
	{
		var providers = new List<Lazy<INuGetResourceProvider>>();
		providers.AddRange(Repository.Provider.GetCoreV3());
		var packageSource = new PackageSource(repository.Source, repository.Name);
		if (repository.IsPrivate)
		{
			packageSource.Credentials = new PackageSourceCredential(repository.Name,
				repository.Username, repository.Password, repository.IsPasswordClearText, null);
		}

		var sourceRepository = new SourceRepository(packageSource, providers);
		return sourceRepository;
	}

	public string GetTargetFramework()
	{
		var frameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
			.OfType<TargetFrameworkAttribute>()
			.Select(x => x.FrameworkName)
			.FirstOrDefault();

		return frameworkName;
	}

	public NuGetFramework GetTargetNugetFramework()
	{
		var frameworkName = GetTargetFramework();
		var currentFramework = frameworkName == null
			? NuGetFramework.AnyFramework
			: NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());
		return currentFramework;
	}

	public FrameworkSpecificGroup GetMostCompatibleGroup(NuGetFramework projectTargetFramework,
		IEnumerable<FrameworkSpecificGroup> itemGroups)
	{
		var reducer                 = new FrameworkReducer();
		var frameworkSpecificGroups = itemGroups.ToList();
		var mostCompatibleFramework = reducer.GetNearest(projectTargetFramework,
			frameworkSpecificGroups.Select(i => i.TargetFramework));

		if (mostCompatibleFramework != null)
		{
			var mostCompatibleGroup =
				frameworkSpecificGroups.FirstOrDefault(i => i.TargetFramework.Equals(mostCompatibleFramework));

			if (IsValid(mostCompatibleGroup))
			{
				return mostCompatibleGroup;
			}
		}

		return null;
	}

	public FrameworkSpecificGroup GetMostCompatibleGroup(IEnumerable<FrameworkSpecificGroup> itemGroups)
	{
		var framework = NugetHelper.Instance.GetTargetNugetFramework();
		return GetMostCompatibleGroup(framework, itemGroups);
	}

	public NuGetFramework GetMostCompatibleFramework(NuGetFramework projectTargetFramework,
		IEnumerable<PackageDependencyGroup> itemGroups)
	{
		var reducer = new FrameworkReducer();
		var mostCompatibleFramework =
			reducer.GetNearest(projectTargetFramework, itemGroups.Select(i => i.TargetFramework));
		return mostCompatibleFramework;
	}

	public NuGetFramework GetMostCompatibleFramework(IEnumerable<PackageDependencyGroup> itemGroups)
	{
		var framework = NugetHelper.Instance.GetTargetNugetFramework();
		return GetMostCompatibleFramework(framework, itemGroups);
	}

	private bool IsValid(NuGet.Packaging.FrameworkSpecificGroup frameworkSpecificGroup)
	{
		if (frameworkSpecificGroup != null)
		{
			return (frameworkSpecificGroup.HasEmptyFolder
			        || frameworkSpecificGroup.Items.Any()
			        || !frameworkSpecificGroup.TargetFramework.Equals(NuGetFramework.AnyFramework));
		}

		return false;
	}

	public List<DllInfo> GetInstallPackagesDllPath(PackageWrapper packageWrapper, ref FolderNuGetProject project)
	{
		var dllInfos = new List<DllInfo>();

		var packageIdentity = packageWrapper.rootPackageIdentity;
		var packageFilePath = project.GetInstalledPackageFilePath(packageIdentity);
		if (!string.IsNullOrWhiteSpace(packageFilePath))
		{
			Logger.LogInformation(packageFilePath);


			var archiveReader = new PackageArchiveReader(packageFilePath, null, null);
			var referenceGroup =
				NugetHelper.Instance.GetMostCompatibleGroup( archiveReader.GetReferenceItems());
			if (referenceGroup != null)
			{
				foreach (var group in referenceGroup.Items)
				{
					var installedPackagedFolder = project.GetInstalledPath(packageIdentity);
					var installedDllPath = Path.Combine(installedPackagedFolder, group);

					var installedDllFolder = Path.GetDirectoryName(installedDllPath);
					var dllName            = Path.GetFileName(installedDllPath);
					var extension          = Path.GetExtension(installedDllPath).ToLower();
					var processor          = group.GetProcessor();

					Logger.LogInformation($"dll Path: {installedDllPath}");

					//check if file path exist , then only add
					if (File.Exists(installedDllPath) && extension == ".dll")
					{
						dllInfos.Add(
							new DllInfo()
							{
								name        = dllName,
								path        = installedDllPath,
								framework   = referenceGroup.TargetFramework.DotNetFrameworkName,
								processor   = processor,
								rootPackage = packageIdentity.Id
							}
						);
					}

					//also , try to cross refer this with expected folder name to avoid version mismatch
				}
			}
		}


		return dllInfos;
	}

	public List<PackageIdentity> GetChildPackageIdentities(IPackageSearchMetadata parentPackage)
	{
		var childPackageIdentities = new List<PackageIdentity>();

		if (parentPackage.DependencySets == null || !parentPackage.DependencySets.Any())
			return childPackageIdentities;

		var mostCompatibleFramework = GetMostCompatibleFramework(parentPackage.DependencySets);
		var dependencyGroup = parentPackage.DependencySets.FirstOrDefault(x => x.TargetFramework == mostCompatibleFramework);
		if (dependencyGroup == null || !dependencyGroup.Packages.Any()) 
			return childPackageIdentities;

		childPackageIdentities.AddRange(dependencyGroup.Packages.Select(package => new PackageIdentity(package.Id, package.VersionRange.MinVersion)));

		return childPackageIdentities;
	}


	public List<DllInfo> GetDllInfoFromDirectory(string directory)
	{
		var d        = new DirectoryInfo(directory);
		var files    = d.GetFiles("*.dll");
		return files.Select(file => new DllInfo() {name = file.Name, path = Path.Combine(directory, file.Name)}).ToList();
	}
}