using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Metatool.Service;
using Microsoft.Win32;
using static Metatool.Service.Key;
using static Metatool.MetaKeyboard.KeyboardConfig;

namespace Metatool.MetaKeyboard
{
    public sealed class Software : CommandPackage
    {

        public Software(ICommandRunner commandRunner, INotify notify, IWindowManager windowManager, IVirtualDesktopManager virtualDesktopManager, IFileExplorer fileExplorer, IConfig<Config> config)
        {
            RegisterCommands();
            var software = config.CurrentValue.SoftwarePackage;
            var hotKeys = software.HotKeys;
            hotKeys.DoublePinyinSwitch.Register(e =>
            {
                e.Handled = true;
                const string keyName   = @"HKEYCURRENTUSER\Software\Microsoft\InputMethod\Settings\CHS";
                const string valueName = "Enable Double Pinyin";
                var          k         = (int) Registry.GetValue(keyName, valueName, -1);
                if (k == 0)
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

            hotKeys.Find.Register(async e =>
            {
                e.Handled = true;
                var shiftDown = e.KeyboardState.IsDown(Shift);

                var c = windowManager.CurrentWindow.Class;
                var arg = shiftDown
                    ? "-newwindow"
                    : "-toggle-window";

                if ("CabinetWClass" == c)
                {
                    var path = await fileExplorer.Path(windowManager.CurrentWindow.Handle);
                    commandRunner.RunWithCmd(commandRunner.NormalizeCmd(Config.Current.Tools.Everything, arg, "-path",
                        path));
                    return;
                }

                commandRunner.RunWithCmd(commandRunner.NormalizeCmd(Config.Current.Tools.Everything, arg));
            });

            hotKeys.OpenTerminal.Register(async e =>
            {
                e.Handled = true;
                var    shiftDown = e.KeyboardState.IsDown(Shift);
                string path;
                var    c = windowManager.CurrentWindow.Class;
                if ("CabinetWClass" != c)
                    path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                else
                    path = await fileExplorer.Path(windowManager.CurrentWindow.Handle);
                if (shiftDown) commandRunner.RunWithCmd(Config.Current.Tools.Terminal, true);
                else commandRunner.RunWithExplorer(Config.Current.Tools.Terminal);
            });

            hotKeys.OpenCodeEditor.Register(async e =>
            {
                if (!windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog)
                {
                    commandRunner.RunWithExplorer(Config.Current.Tools.Code);
                    return;
                }

                var paths = await fileExplorer.GetSelectedPath(windowManager.CurrentWindow.Handle);

                if (paths.Length == 0)
                {
                    var path = await fileExplorer.Path(windowManager.CurrentWindow.Handle);
                    commandRunner.RunWithExplorer(Config.Current.Tools.Code, path);
                    return;
                }

                foreach (var path in paths)
                {
                    commandRunner.RunWithCmd(commandRunner.NormalizeCmd(Config.Current.Tools.Code, path));
                }
            });
            hotKeys.WebSearch.Register(async e =>
            {
                e.Handled = true;

                var altDown = e.KeyboardState.IsDown(Keys.Menu);
                var url = altDown
                    ? Config.Current.Tools.SearchEngineSecondary
                    : Config.Current.Tools.SearchEngine;

                var defaultPath = Browser.DefaultPath;
                var exeName     = Path.GetFileNameWithoutExtension(defaultPath);
                var process     = await virtualDesktopManager.GetFirstProcessOnCurrentVirtualDesktop(exeName);
                if (process == null)
                {
                    commandRunner.RunAsNormalUser(defaultPath, url, "--new-window", "--new-instance");
                    return;
                }

                new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = true,
                        FileName        = url
                    }
                }.Start();
            });
            hotKeys.StartTaskExplorer.WithAliases(software.KeyAliases).Register(e =>
            {
                e.Handled = true;
                commandRunner.RunWithCmd(Config.Current.Tools.ProcessExplorer);
            });

            hotKeys.OpenScreenRuler.WithAliases(software.KeyAliases).Register(e =>
            {
                e.Handled = true;
                commandRunner.RunWithCmd(Config.Current.Tools.Ruler);
            });

            hotKeys.OpenScreenRuler.WithAliases(software.KeyAliases).Register(async e =>
            {
                var exeName = "Inspect";

                var processes = await
                    virtualDesktopManager.GetProcessesOnCurrentVirtualDesktop(exeName);

                var process = processes.FirstOrDefault();

                var hWnd = process?.MainWindowHandle;

                if (hWnd != null)
                {
                    windowManager.Show(hWnd.Value);
                    return;
                }

                commandRunner.RunWithExplorer(Config.Current.Tools.Inspect);
            });

            hotKeys.StartNotepad.WithAliases(software.KeyAliases).Register(async e =>
            {
                e.Handled = true;
                var exeName = "Notepad";

                var notePads = await
                    virtualDesktopManager.GetProcessesOnCurrentVirtualDesktop(exeName,
                        p => p.MainWindowTitle == "Untitled - Notepad");

                var notePad = notePads.FirstOrDefault();

                var hWnd = notePad?.MainWindowHandle;

                if (hWnd != null)
                {
                    windowManager.Show(hWnd.Value);
                    return;
                }

                commandRunner.RunWithCmd("Notepad");
            });

            hotKeys.StartVisualStudio.WithAliases(software.KeyAliases).Register(async e =>
            {
                if (!windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog) return;

                e.Handled = true;

                var path = await fileExplorer.Path(windowManager.CurrentWindow.Handle);
                if (string.IsNullOrEmpty(path))
                {
                    commandRunner.RunWithExplorer(Config.Current.Tools.VisualStudio);
                    return;
                }

                Directory.CreateDirectory(path).EnumerateFiles("*.sln").Select(f => f.FullName).AsParallel().ForAll(s =>
                {
                    Process.Start(new ProcessStartInfo(Config.Current.Tools.VisualStudio)
                        {UseShellExecute = true, Arguments = s, WorkingDirectory = path});
                });
            });

            hotKeys.StartGifRecord.WithAliases(software.KeyAliases).Register(e =>
            {
                e.Handled = true;
                commandRunner.RunWithCmd(Config.Current.Tools.GifTool);
            });

        }

        public IKeyCommand ToggleDictionary = (AK + D).MapOnHit(Shift + LAlt + D);

    }
}