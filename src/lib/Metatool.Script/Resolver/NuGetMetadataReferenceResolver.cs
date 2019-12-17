using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace Metatool.Metatool.Script.Resolver
{
       public class NuGetMetadataReferenceResolver : MetadataReferenceResolver
    {
        private readonly bool _useCache;
        private readonly MetadataReferenceResolver _innerReferenceResolver;
        private readonly ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>> _cache;

        public NuGetMetadataReferenceResolver(string workingDirectory,MetadataReferenceResolver innerReferenceResolver=null, bool useCache = false)
        {
            _useCache = useCache;
            _innerReferenceResolver = innerReferenceResolver ?? ScriptMetadataResolver.Default.WithBaseDirectory(workingDirectory);
            if (useCache)
            {
                _cache = new ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>>();
            }
        }
        
        public override bool Equals(object other) => _innerReferenceResolver.Equals(other);

        public override int GetHashCode() => _innerReferenceResolver.GetHashCode();

        public override bool ResolveMissingAssemblies => _innerReferenceResolver.ResolveMissingAssemblies;

        public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
        {
            if (_cache == null) return _innerReferenceResolver.ResolveMissingAssembly(definition, referenceIdentity);
            return _cache.GetOrAdd(referenceIdentity.ToString(),
                _ => ImmutableArray.Create(_innerReferenceResolver.ResolveMissingAssembly(definition, referenceIdentity))).FirstOrDefault();
        }


        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {
            if (reference.StartsWith("nuget:", StringComparison.OrdinalIgnoreCase))
            {
                // HACK We need to return something here to "mark" the reference as resolved. 
                // https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/ReferenceManager/CommonReferenceManager.Resolution.cs#L838
                return ImmutableArray<PortableExecutableReference>.Empty.Add(
                    MetadataReference.CreateFromFile(typeof(NuGetMetadataReferenceResolver).GetTypeInfo().Assembly.Location));
            }

            if (_cache == null) return _innerReferenceResolver.ResolveReference(reference, baseFilePath, properties); 

            if (_cache.TryGetValue(reference, out var result)) return result;

            result = _innerReferenceResolver.ResolveReference(reference, baseFilePath, properties);
            if (!result.IsDefaultOrEmpty)
            {
                _cache.TryAdd(reference, result);
            }

            return result;
        }
    }
}
