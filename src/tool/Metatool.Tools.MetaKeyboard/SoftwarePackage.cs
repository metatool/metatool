using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Microsoft.Win32;
using static Metatool.Service.MouseKey.Key;

namespace Metatool.MetaKeyboard
{
    public sealed class Software : CommandPackage
    {
        public Software(IShell shell, INotify notify, IWindowManager windowManager, IVirtualDesktopManager virtualDesktopManager, IFileExplorer fileExplorer, IConfig<Config> config)
        {
            RegisterCommands();
            var software = config.CurrentValue.SoftwarePackage;
            var hotKeys = software.Hotkeys;
            var swPaths = software.SoftwarePaths;
            hotKeys.DoublePinyinSwitch.OnEvent(e =>
            {
                e.Handled = true;
                const string keyName   = @"HKEY_CURRENT_USER\Software\Microsoft\InputMethod\Settings\CHS";
                const string valueName = "Enable Double Pinyin";
                var          k         = (int) Registry.GetValue(keyName, valueName, -1);
                if (k == 0 || k == -1)
                {
                    notify.ShowMessage("Double Pinyin Enabled");
                    Registry.SetValue(keyName, valueName, 1);
                }
                else if (k == 1)
                {
                    notify.ShowMessage("Full Pinyin Enabled");
                    Registry.SetValue(keyName, valueName, 0);
                }
            });

            //hotKeys.Find.OnEvent(async e =>
            //{
            //    e.Handled = true;
            //    var shiftDown = e.KeyboardState.IsDown(Shift);

            //    var c = windowManager.CurrentWindow.Class;
            //    var arg = shiftDown
            //        ? "-newwindow"
            //        : "-toggle-window";

            //    if ("CabinetWClass" == c)
            //    {
            //        var path = await fileExplorer.CurrentDirectory(windowManager.CurrentWindow.Handle);
            //        shell.RunWithCmd(shell.NormalizeCmd(swPaths.Everything, arg, "-path",
            //            path));
            //        return;
            //    }

            //    shell.RunWithCmd(shell.NormalizeCmd(swPaths.Everything, arg));
            //});

            //hotKeys.OpenTerminal.OnEvent(async e =>
            //{
            //    e.Handled = true;
            //    var    shiftDown = e.KeyboardState.IsDown(Shift);
            //    string path;
            //    var    c = windowManager.CurrentWindow.Class;
            //    if ("CabinetWClass" != c)
            //        path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            //    else
            //        path = await fileExplorer.CurrentDirectory(windowManager.CurrentWindow.Handle);
            //    // https://github.com/nt4f04uNd/wt-contextmenu
            //    if (shiftDown) shell.RunWithCmd(swPaths.Terminal, true); // powershell -Command "Start-Process shell:appsFolder\Microsoft.WindowsTerminal_8wekyb3d8bbwe!App -Verb RunAs"
            //    else shell.RunWithExplorer(swPaths.Terminal);
            //});

            // hotKeys.OpenCodeEditor.OnEvent(async e =>
            // {
            //     e.Handled = true;
            //     if (!windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog)
            //     {
            //         shell.RunWithExplorer(swPaths.Code);
            //         return;
            //     }
            //
            //     var paths = await fileExplorer.GetSelectedPaths(windowManager.CurrentWindow.Handle);
            //
            //     if (paths.Length == 0)
            //     {
            //         var path = await fileExplorer.Path(windowManager.CurrentWindow.Handle);
            //         shell.RunWithCmd(shell.NormalizeCmd(swPaths.Code, path));
            //         return;
            //     }
            //
            //     foreach (var path in paths)
            //     {
            //         shell.RunWithCmd(shell.NormalizeCmd(swPaths.Code, path));
            //     }
            // });

            //hotKeys.WebSearch.OnEvent(async e =>
            //{
            //    e.Handled = true;

            //    var altDown = e.KeyboardState.IsDown(Keys.Menu);
            //    var url = altDown
            //        ? swPaths.SearchEngineSecondary
            //        : swPaths.SearchEngine;

            //    var defaultPath = Browser.DefaultPath;
            //    var exeName     = Path.GetFileNameWithoutExtension(defaultPath);
            //    var process     = await virtualDesktopManager.GetFirstProcessOnCurrentVirtualDesktop(exeName);
            //    if (process == null)
            //    {
            //        shell.RunAsNormalUser(defaultPath, url, "--new-window", "--new-instance");
            //        return;
            //    }

            //    new Process
            //    {
            //        StartInfo =
            //        {
            //            UseShellExecute = true,
            //            FileName        = url
            //        }
            //    }.Start();
            //});
            // hotKeys.StartTaskExplorer.WithAliases(software.KeyAliases).OnEvent(e =>
            // {
            //     e.Handled = true;
            //     shell.RunWithCmd(swPaths.ProcessExplorer);
            // });
            //
            // hotKeys.OpenScreenRuler.WithAliases(software.KeyAliases).OnEvent(e =>
            // {
            //     e.Handled = true;
            //     shell.RunWithCmd(swPaths.Ruler);
            // });

            //hotKeys.OpenScreenRuler.WithAliases(software.KeyAliases).OnEvent(async e =>
            //{
            //    var exeName = "Inspect";

            //    var processes = await
            //        virtualDesktopManager.GetProcessesOnCurrentVirtualDesktop(exeName);

            //    var process = processes.FirstOrDefault();

            //    var hWnd = process?.MainWindowHandle;

            //    if (hWnd != null)
            //    {
            //        windowManager.Show(hWnd.Value);
            //        return;
            //    }

            //    shell.RunWithExplorer(swPaths.Inspect);
            //});

            //hotKeys.StartNotepad.WithAliases(software.KeyAliases).OnEvent(async e =>
            //{
            //    e.Handled = true;
            //    var exeName = "Notepad";

            //    var notePads = await
            //        virtualDesktopManager.GetProcessesOnCurrentVirtualDesktop(exeName,
            //            p => p.MainWindowTitle == "Untitled - Notepad");

            //    var notePad = notePads.FirstOrDefault();

            //    var hWnd = notePad?.MainWindowHandle;

            //    if (hWnd != null)
            //    {
            //        windowManager.Show(hWnd.Value);
            //        return;
            //    }

            //    shell.RunWithCmd("Notepad");
            //});

            hotKeys.StartVisualStudio.WithAliases(software.KeyAliases).OnEvent(async e =>
            {
                if (!windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog) return;

                e.Handled = true;

                var path = await fileExplorer.CurrentDirectory(windowManager.CurrentWindow.Handle);
                if (string.IsNullOrEmpty(path))
                {
                    shell.RunWithExplorer(swPaths.VisualStudio);
                    return;
                }
                Directory.CreateDirectory(path).EnumerateFiles("*.sln").Select(f => f.FullName).AsParallel().ForAll(s =>
                {
                    Process.Start(new ProcessStartInfo(swPaths.VisualStudio)
                        {UseShellExecute = true, Arguments = s, WorkingDirectory = path});
                });
            });

            // hotKeys.StartGifRecord.WithAliases(software.KeyAliases).OnEvent(e =>
            // {
            //     e.Handled = true;
            //     shell.RunWithCmd(swPaths.GifTool);
            // });

            hotKeys.ToggleDictionary.MapOnAllUp(Shift + LAlt + D, tree: KeyStateTrees.Map);
        }

    }
}