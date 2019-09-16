using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using Metaseed.Metatool.Script.Resolver;
using Metaseed.Script.NugetReference;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Metaseed.Script
{
    static class Program
    {
        static void Main(string[] args)
        {
            var path = Path.Combine(AppContext.BaseDirectory,"BBB", "cc.csx");
            new ScriptHost(null).Build(path);
        }
    }
}
