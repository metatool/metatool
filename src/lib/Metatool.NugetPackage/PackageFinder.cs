using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Metatool.NugetPackage
{
    public class PackageFinder
    {
        private          string                        _targetFramework;
        private readonly ILogger                       _logger;

        public PackageFinder()
        {
            _targetFramework = NugetHelper.Instance.GetTargetFramework();
            _logger          = NugetHelper.Instance.Logger;
            
        }

        public PackageWrapper GetPackageByExactSearch(string packageName, string version, IList<NugetRepository> repositories, bool disableCache = false)
        {
            PackageWrapper packageWrapper = null;
            var sourceRepos = NugetHelper.Instance.GetSourceRepos(repositories);
            foreach (var sourceRepository in sourceRepos)
            {
                    var sourceCacheContext = new SourceCacheContext();
                    // below will slow down search as it is disabling search
                    if (disableCache)
                    {
                        sourceCacheContext.NoCache        = true;
                        sourceCacheContext.DirectDownload = true;
                    }

                    var packageMetadataResource = sourceRepository.GetResourceAsync<PackageMetadataResource>().Result;

                    IPackageSearchMetadata package = null;

                    if (!string.IsNullOrWhiteSpace(version))
                    {
                        package = GetPackageFromRepoWithVersion(packageName, version,
                            packageMetadataResource, sourceCacheContext, sourceRepository);
                    }
                    else
                    {
                        package = GetPackageFromRepoWithoutVersion(packageName,
                            packageMetadataResource, sourceCacheContext, sourceRepository);
                    }

                    if (package == null)
                    {
                        _logger.LogInformation($" No Package found in Repo " +
                                               $"{sourceRepository.PackageSource.Source} for package : {packageName} | {version}");
                        continue;
                    }

                    packageWrapper = new PackageWrapper
                    {
                        rootPackageIdentity = package.Identity,
                        packageName         = package.Identity.Id,
                        version             = package.Identity.Version,
                        //save the repo info as well so that during install it doesn't need to search on all repos
                        sourceRepository = sourceRepository,
                        //load child package identities
                        childPackageIdentities = NugetHelper.Instance.GetChildPackageIdentities(package)
                    };

                    _logger.LogInformation($"Latest Package form Exact Search : {packageWrapper.packageName}" +
                                           $"| {packageWrapper.version} in Repo {sourceRepository.PackageSource.Source}");
                    break;
            }

            return packageWrapper;
        }

        public IPackageSearchMetadata GetPackageFromRepoWithVersion(string packageName, string version,
            PackageMetadataResource packageMetadataResource, SourceCacheContext sourceCacheContext,
            SourceRepository sourceRepository)
        {
            if (!NuGetVersion.TryParse(version, out _)) return null;

            IPackageSearchMetadata rootPackage     = null;
            var                    packageIdentity = new PackageIdentity(packageName, NuGetVersion.Parse(version));
            var exactSearchMetadata = packageMetadataResource
                .GetMetadataAsync(packageIdentity, sourceCacheContext, _logger, CancellationToken.None).Result;

            if (exactSearchMetadata == null)
            {
                _logger.LogInformation($"GetPackageFromRepoWithVersion - No Package found in Repo " +
                                       $"{sourceRepository.PackageSource.Source} for package : {packageName}  with version  {version}");
            }
            else
            {
                rootPackage = exactSearchMetadata;
            }

            return rootPackage;
        }

        public IPackageSearchMetadata GetPackageFromRepoWithoutVersion(
            string packageName,
            PackageMetadataResource packageMetadataResource,
            SourceCacheContext sourceCacheContext,
            SourceRepository sourceRepository)
        {
            IPackageSearchMetadata rootPackage = null;
            var exactSearchMetadata = packageMetadataResource
                .GetMetadataAsync(packageName, true, true, sourceCacheContext, _logger, CancellationToken.None).Result
                .ToList();

            if (!exactSearchMetadata.Any())
            {
                _logger.LogInformation($"GetPackageFromRepoWithoutVersion - No Package & any version  found in Repo " +
                                       $"{sourceRepository.PackageSource.Source} for package : {packageName}");
            }
            else
            {
                rootPackage = exactSearchMetadata.OrderByDescending(x => x.Identity.Version)
                    .FirstOrDefault();
            }

            return rootPackage;
        }
    }
}