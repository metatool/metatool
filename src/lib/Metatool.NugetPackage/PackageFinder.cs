using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Metatool.NugetPackage
{
    public class PackageFinder
    {
        private          string  _targetFramework;
        private readonly ILogger _logger;

        public PackageFinder()
        {
            _targetFramework = NugetHelper.Instance.GetTargetFramework();
            _logger          = NugetHelper.Instance.Logger;
        }

        public async Task<PackageWrapper> GetPackageByExactSearch(string packageName,
            IList<NugetRepository> repositories,
            string version, bool disableCache = false, bool includePrerelease = false,
            bool includeUnlisted = false)
        {
            var sourceRepos = NugetHelper.Instance.GetSourceRepos(repositories);
            foreach (var sourceRepository in sourceRepos)
            {
                IPackageSearchMetadata package;
                version = version?.Trim();
                if (!string.IsNullOrEmpty(version) || !string.Equals("*", version))
                {
                    package = await GetPackage(packageName, version, sourceRepository, disableCache);
                }
                else
                {
                    package = await GetLatestPackage(packageName, sourceRepository, includePrerelease, includeUnlisted,
                        disableCache);
                }

                if (package == null)
                {
                    _logger.LogInformation($" No Package found in Repo " +
                                           $"{sourceRepository.PackageSource.Source} for package : {packageName} | {version}");
                    continue;
                }

                var packageWrapper = new PackageWrapper
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
                return packageWrapper;
            }

            return null;
        }

        public async Task<IPackageSearchMetadata> GetPackage(string packageName, string version,
            SourceRepository sourceRepository,
            bool disableCache
        )
        {
            if (!NuGetVersion.TryParse(version, out var ver)) return null;
            var packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
            var sourceCacheContext = new SourceCacheContext()
                {NoCache = disableCache, DirectDownload = disableCache};
            var packageIdentity = new PackageIdentity(packageName, ver);
            var exactSearchMetadata = await packageMetadataResource
                .GetMetadataAsync(packageIdentity, sourceCacheContext, _logger, CancellationToken.None);

            if (exactSearchMetadata == null)
            {
                _logger.LogInformation($"GetPackageFromRepoWithVersion - No Package found in Repo " +
                                       $"{sourceRepository.PackageSource.Source} for package : {packageName}  with version  {version}");
            }

            return exactSearchMetadata;
        }

        public async Task<(IPackageSearchMetadata metadata, PackageSource source)> GetLatestPackage(
            string packageName, IEnumerable<SourceRepository> sourceRepositories,
            bool includePrerelease, bool includeUnlisted,
            bool disableCache)
        {
            IPackageSearchMetadata packageMetadata = null;
            PackageSource          sourceRepo      = null;
            // var                    sourceRepos     = NugetHelper.Instance.GetSourceRepos(repositories);
            foreach (var sourceRepository in sourceRepositories)
            {
                var metadata = await GetLatestPackage(packageName, sourceRepository, includePrerelease, includeUnlisted,
                    disableCache);
                if (packageMetadata == null                                                                  ||
                    metadata != null && metadata.Identity.HasVersion && !packageMetadata.Identity.HasVersion ||
                    metadata != null                    && metadata.Identity.HasVersion &&
                    packageMetadata.Identity.HasVersion &&
                    metadata.Identity.Version > packageMetadata.Identity.Version)
                {
                    packageMetadata = metadata;
                    sourceRepo      = sourceRepository.PackageSource;
                }
            }

            return (packageMetadata, sourceRepo);
        }

        public async Task<IPackageSearchMetadata> GetLatestPackage(
            string packageName, SourceRepository sourceRepository,
            bool includePrerelease, bool includeUnlisted,
            bool disableCache)
        {
            var packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();

            var sourceCacheContext = new SourceCacheContext()
                {NoCache = disableCache, DirectDownload = disableCache};

            var exactSearchMetadata = (await packageMetadataResource
                .GetMetadataAsync(packageName, includePrerelease, includeUnlisted, sourceCacheContext, _logger,
                    CancellationToken.None)).ToList();

            if (!exactSearchMetadata.Any())
            {
                _logger.LogInformation(
                    $"GetLatestPackage - No Package & any version  found in Repo {sourceRepository.PackageSource.Source} for package : {packageName}");
                return null;
            }
            else
            {
                _logger.LogInformation(
                    $"GetLatestPackage - Package found in Repo {sourceRepository.PackageSource.Source} for package : {packageName}");

                var rootPackage = exactSearchMetadata.OrderByDescending(x => x.Identity.Version)
                    .FirstOrDefault();
                return rootPackage;
            }
        }

    }
}