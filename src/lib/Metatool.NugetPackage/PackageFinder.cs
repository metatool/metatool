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
        public           IEnumerable<SourceRepository> _sourceRepos { get; set; }


        public PackageFinder(IList<NugetRepository> repositories)
        {
            _targetFramework = NugetHelper.Instance.GetTargetFramework();
            _logger          = NugetHelper.Instance.Logger;
            _sourceRepos     = NugetHelper.Instance.GetSourceRepos(repositories);
        }

        public PackageWrapper GetPackageByExactSearch(string packageName, string version, bool disableCache = false)
        {
            var            packageFound   = false;
            PackageWrapper packageWrapper = null;

            #region processing 

            foreach (var sourceRepository in _sourceRepos)
            {
                if (!packageFound)
                {
                    var packageMetadataResource = sourceRepository
                        .GetResourceAsync<PackageMetadataResource>().Result;

                    var sourceCacheContext = new SourceCacheContext();

                    ////below will slow down search as it is disabling search
                    if (disableCache)
                    {
                        sourceCacheContext.NoCache        = true;
                        sourceCacheContext.DirectDownload = true;
                    }

                    IPackageSearchMetadata rootPackage = null;

                    //if user has mentioned version , then search specifcially for that version only , else get latest version
                    if (!string.IsNullOrWhiteSpace(version))
                    {
                        rootPackage = GetPackageFromRepoWithVersion(packageName, version,
                            packageMetadataResource, sourceCacheContext, sourceRepository);
                    }
                    else
                    {
                        rootPackage = GetPackageFromRepoWithoutVersion(packageName,
                            packageMetadataResource, sourceCacheContext, sourceRepository);
                    }

                    if (rootPackage == null)
                    {
                        _logger.LogInformation($" No Package found in Repo " +
                                               $"{sourceRepository.PackageSource.Source} for package : {packageName} | {version}");

                        //as we have not found package , there is no need to process further ,look for next repo by continue
                        continue;
                    }
                    else
                    {
                        packageWrapper = new PackageWrapper
                        {
                            rootPackageIdentity = rootPackage.Identity,
                            packageName = rootPackage.Identity.Id,
                            version = rootPackage.Identity.Version,
                            //save the repo info as well so that during install it doesn't need to search on all repos
                            sourceRepository = sourceRepository,
                            //load child package identities
                            childPackageIdentities =
                                NugetHelper.Instance.GetChildPackageIdentities(rootPackage)
                        };

                        _logger.LogInformation($"Latest Package form Exact Search : {packageWrapper.packageName}" +
                                               $"| {packageWrapper.version} in Repo {sourceRepository.PackageSource.Source}");
                    }

                    packageFound = true;
                    //as package is found , we can break loop here for this package, but keeping above bool as well for testing

                    break;
                }
            }

            #endregion

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