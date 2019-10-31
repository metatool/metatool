using System.Collections.Generic;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Metatool.NugetPackage
{
    public class PackageWrapper
    {
        public string                packageName            { get; set; }
        public NuGetVersion          version                { get; set; }
        public PackageIdentity       rootPackageIdentity    { get; set; }
        public List<PackageIdentity> childPackageIdentities { get; set; }

        public SourceRepository sourceRepository { get; set; }
        public string           PossibleFolder   => $"{packageName}.{version.Version.ToString()}";

        public class SamePackageAndVersion : IEqualityComparer<PackageWrapper>
        {
            public bool Equals(PackageWrapper P1, PackageWrapper P2)
            {
                if ((P1.packageName == P2.packageName) && (P1.version == P2.version))
                    return true;
                else
                    return false;
            }


            public int GetHashCode(PackageWrapper P0)
            {
                return P0.packageName.GetHashCode() + P0.version.GetHashCode();
            }
        }
    }
}