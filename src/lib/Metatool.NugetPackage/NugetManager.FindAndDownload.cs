using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Metatool.NugetPackage
{
    public partial class NugetManager
    {
        readonly PackageDownloader _packageDownloader = new PackageDownloader();
        readonly PackageFinder     _packageFinder      = new PackageFinder();

        public List<PackageWrapper> GetListOfPackageIdentities(string packageName, string version,
            IList<NugetRepository> repositories, string folder,
            bool disableCache = false, bool includePrerelease = false, bool allowUnlisted = false)
        {
            var packageDownloadTasks = new List<Task<List<DllInfo>>>();
            var packageWrappers      = new List<PackageWrapper>();

            void GetListOfPackageIdentitiesRecursive(string pkgName, string ver)
            {
                var packageWrapper = _packageFinder.GetPackageByExactSearch(pkgName, repositories, ver, disableCache, includePrerelease, allowUnlisted).Result;
                if (packageWrapper == null)
                {
                    return;
                }

                Parallel.ForEach(packageWrapper.childPackageIdentities, childPackageIdentity =>
                {
                    if (!packageWrappers.Any(x => string.Equals(x.packageName, childPackageIdentity.Id,
                        StringComparison.CurrentCultureIgnoreCase)))
                    {
                        GetListOfPackageIdentitiesRecursive(childPackageIdentity.Id,
                            childPackageIdentity.Version.Version.ToString());
                    }
                });

                packageDownloadTasks.Add(_packageDownloader.DownloadPackage(packageWrapper, folder, disableCache,
                    includePrerelease, allowUnlisted));
                lock (packageWrappers)
                {
                    packageWrappers.Add(packageWrapper);
                }
            }

            GetListOfPackageIdentitiesRecursive(packageName, version);

            var continuation = Task.WhenAll(packageDownloadTasks);
            try
            {
                continuation.Wait();
            }
            catch (AggregateException e)
            {
                _logger.LogError(e.Flatten().Message);
            }

            var dllInfos = new List<DllInfo>();

            if (continuation.Status == TaskStatus.RanToCompletion)
            {
                foreach (var result in continuation.Result)
                {
                    dllInfos.AddRange(result);
                }
            }
            else
            {
                foreach (var t in packageDownloadTasks)
                {
                    _logger.LogWarning($"{t.Id}: {t.Status}");
                }
            }

            packageWrappers = packageWrappers.DistinctBy(x => x.packageName).ToList();
            dllInfos        = dllInfos.DistinctBy(x => x.path).ToList();

            return packageWrappers;
        }
    }
}
