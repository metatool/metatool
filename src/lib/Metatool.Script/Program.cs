﻿using System;
using System.IO;
using System.Threading.Tasks;
using static Metatool.Metatool.SimpleConsoleLoggerProvider;

namespace Metatool.Script
{
    static class Program
    {
        const string ScriptBin = "bin";

        static async Task Main(string[] args)
        {
            // var path = Path.Combine(AppContext.BaseDirectory,"BBB", "cc.csx");
            var scriptPath = args[0];
            if (!File.Exists(scriptPath))
            {
                Console.WriteLine("the script path is not right!");
                return;
            }
            if (!scriptPath.EndsWith(".csx"))
            {
                Console.WriteLine("the script should be end with '.csx'!");
                return;
            }
            var assemblyName = Path.GetFileNameWithoutExtension(scriptPath);
            if (args.Length > 1)
            {
                assemblyName = args[1];
            }

            var outputDir = Path.Combine(Path.GetDirectoryName(scriptPath), ScriptBin);
            // var dll = Path.Combine(outputDir, assemblyName + ".dll");

            var logger = new SimpleConsoleLogger(nameof(ScriptHost));
            await new ScriptHost(logger).Build(scriptPath, outputDir, assemblyName, onlyBuild:false);
            
        }
    }
}
