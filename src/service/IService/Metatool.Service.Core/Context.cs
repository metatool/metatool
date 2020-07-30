using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Threading;

namespace Metatool.Service
{
    public class Context
    {
        public static Dispatcher Dispatcher;
        public static bool IsElevated =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        private static string _dotnetExePath;
        private static readonly string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        public static string DotnetExePath =>
            _dotnetExePath ??= string.IsNullOrEmpty(dotnetRoot) ? "" : Path.Combine(dotnetRoot, "dotnet.exe");

        // real path exe that is running
        public static string BaseDirectory => AppContext.BaseDirectory;

        static string _appDirectory;

        // singleton exe filt path
        public static string AppDirectory
        {
            get
            {
                if (Debugger.IsAttached) return BaseDirectory;

                if (_appDirectory != null) return _appDirectory;
                var mainModule = Process.GetCurrentProcess().MainModule;
                _appDirectory = Path.GetDirectoryName(mainModule?.FileName);
                return _appDirectory;
            }
        }

        public static string ParsePath(string rawPath, string currentDir = null, Type toolType = null)
        {
            if (toolType != null)
            {
                var toolDir = Path.GetDirectoryName(toolType.Assembly.Location);
                rawPath = rawPath.Replace("${toolDir}", toolDir, StringComparison.InvariantCultureIgnoreCase);
            }
            var path = rawPath.Replace("${appDir}", AppDirectory, StringComparison.InvariantCultureIgnoreCase);

            if (path.StartsWith(".\\") || path.StartsWith("./") || path.StartsWith("..\\") || path.StartsWith("../"))
            {
                currentDir ??= AppDirectory;
                path = Path.GetFullPath(Path.Combine(currentDir, path));
            }

            return path;
        }

        public static string ToolDir<T>() => Path.GetDirectoryName(typeof(T).Assembly.Location);

        public static string CurrentDirectory => Environment.CurrentDirectory;

        public static string DefaultToolsDirectory => Path.Combine(AppDirectory, "tools");
        public static string PackageDirectory => Path.Combine(BaseDirectory, ".pkg");
        public static string PackageSourceDirectory => Path.Combine(AppDirectory, "pkg");

        public static void Exit(int code)
        {
            Dispatcher?.BeginInvoke((Action)(() => Application.Current.Shutdown(code)));
        }

        public static int Restart(int code, bool admin)
        {
            void restart()
            {
                var p = System.Windows.Forms.Application.ExecutablePath;
                var path = Path.GetFileNameWithoutExtension(p);
                try
                {
                    var process = new Process()
                    {
                        StartInfo =
                        {
                            FileName        = path,
                            UseShellExecute = true,
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
            }
            if (Dispatcher == null) restart();
            else Dispatcher?.BeginInvoke((Action)restart);
            return code;

        }
    }
}
