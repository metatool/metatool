using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Metatool.Plugin
{
    public class Context
    {
        public static Dispatcher Dispatcher;
        public static bool IsElevated =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
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

        public static string DefaultToolsDirectory  => Path.Combine(AppDirectory, "tools");
        public static string PackageDirectory       => Path.Combine(BaseDirectory, ".pkg");
        public static string PackageSourceDirectory => Path.Combine(AppDirectory, "pkg");

        public static void Exit(int code)
        {
            Dispatcher?.BeginInvoke((Action) (() => Application.Current.Shutdown(code)));
        }

        public static void Restart(int code, bool admin)
        {
            Dispatcher?.BeginInvoke((Action)(() =>
            {

                var p = System.Windows.Forms.Application.ExecutablePath;
                var path = p.Remove(p.Length - 4, 4) + ".exe";
                try
                {
                    var process = new Process()
                    {
                        StartInfo =
                        {
                            FileName        = path,
                            UseShellExecute = true
                        }
                    };

                    if (admin) process.StartInfo.Verb = "runas";
                    process.Start();
                    Application.Current.Shutdown(code);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }));
          
           
        }
    }
}