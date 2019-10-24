using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Metatool.Plugin
{
    public class Context
    {
        private static          string _dotnetExePath;
        private static readonly string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        public static string DotnetExePath =>
            _dotnetExePath ??= string.IsNullOrEmpty(dotnetRoot) ? "" : Path.Combine(dotnetRoot, "dotnet.exe");

        public static string BaseDirectory => AppContext.BaseDirectory;

        public static string _appDirectory;

        public static string AppDirectory
        {
            get
            {
                if (_appDirectory != null) return _appDirectory;
                var mainModule = Process.GetCurrentProcess().MainModule;
                _appDirectory = Path.GetDirectoryName(mainModule?.FileName);
                return _appDirectory;
            }
        }

        public static string ToolDir<T>() => Path.GetDirectoryName(typeof(T).Assembly.Location);

        public static string CurrentDirectory => Environment.CurrentDirectory;
    }

        
}