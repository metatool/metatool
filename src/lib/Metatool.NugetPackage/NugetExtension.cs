using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Metatool.NugetPackage
{
    public static class NugetExtension
    {
        public static string GetProcessor(this string folder)
        {
            if(folder.ToLower().Contains("x86"))
            {
                return "x86";
            }
            else if (folder.ToLower().Contains("x64"))
            {
                return "x64";
            }
            else
            {
                return "NA";
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
                this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            return source.Where(element => knownKeys.Add(keySelector(element)));
        }
    }
}
