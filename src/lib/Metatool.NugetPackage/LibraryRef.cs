using System;
using System.Collections.Generic;
using System.Text;
using NuGet.Versioning;

namespace Metatool.NugetPackage
{
    public class LibraryRef : IEquatable<LibraryRef>
    {
        public LibraryRef(string id, VersionRange versionRange)
        {
            Id           = id;
            VersionRange = versionRange;
        }

        public LibraryRef(string path) : this(string.Empty, VersionRange.AllFloating)
        {
            Path         = path;
            AssemblyName = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public string       Id           { get; }
        public VersionRange VersionRange { get; }
        public string      Path         { get; }
        public string      AssemblyName { get; }

        public bool Equals(LibraryRef? other)
        {
            return other != null &&
                   (Id, VersionRange, Path).Equals((other.Id, other.VersionRange, other.Path));
        }

        public override bool Equals(object obj) => Equals(obj as LibraryRef);

        public override int GetHashCode()
        {
            return (Id, VersionRange).GetHashCode();
        }
    }
}
