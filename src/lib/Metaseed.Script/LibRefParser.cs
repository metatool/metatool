using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Metaseed.Metatool.Script.Resolver;
using Metaseed.Script.NugetReference;
using Microsoft.CodeAnalysis.CSharp;
using NuGet.Versioning;

namespace Metaseed.Script
{
    public static class LibRefParser
    {
        public static List<LibraryRef> ParseReference(string code)
        {
            var compilation = SyntaxFactory.ParseCompilationUnit(code);
            var libraries   = new List<LibraryRef>();
            var matcher     = NuGetSourceReferenceResolver.PackageNameVersionMatcher;
            bool IsLocalReference(string path)
            {
                switch (Path.GetExtension(path)?.ToLowerInvariant())
                {
                    // add a "project" reference if it's not a GAC reference
                    case ".dll":
                    case ".exe":
                    case ".winmd":
                        return true;
                }

                return false;
            }

            foreach (var directive in compilation.GetReferenceDirectives())
            {
                var path = directive.File.ValueText;
                if (path.StartsWith("nuget:", StringComparison.OrdinalIgnoreCase))
                {
                    var group = matcher.Match(path).Groups;
                    var (name, version) = (group[1].Value, group[2].Value);

                    var versionRange = version == "" ? VersionRange.All : VersionRange.Parse(version);
                    libraries.Add(new LibraryRef(name, versionRange));
                }
                else if(IsLocalReference(path))
                {
                    libraries.Add(new LibraryRef(path));
                }
            }

            return libraries;
        }
    }
}