﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Metatool.Service;
using Microsoft.Win32;
using static Metatool.Service.Key;

namespace Metatool.MetaKeyboard
{
    public sealed class Software : CommandPackage
    {
        public Software(ICommandRunner commandRunner, INotify notify, IWindowManager windowManager, IVirtualDesktopManager virtualDesktopManager, IFileExplorer fileExplorer, IConfig<Config> config)
        {
            RegisterCommands();
            var software = config.CurrentValue.SoftwarePackage;
            var hotKeys = software.HotKeys;
            var swPaths = software.SoftwarePaths;
            hotKeys.DoublePinyinSwitch.Event(e =>
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

            hotKeys.Find.Event(async e =>
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
                    commandRunner.RunWithCmd(commandRunner.NormalizeCmd(swPaths.Everything, arg, "-path",
                        path));
                    return;
                }

                commandRunner.RunWithCmd(commandRunner.NormalizeCmd(swPaths.Everything, arg));
            });

            hotKeys.OpenTerminal.Event(async e =>
            {
                e.Handled = true;
                var    shiftDown = e.KeyboardState.IsDown(Shift);
                string path;
                var    c = windowManager.CurrentWindow.Class;
                if ("CabinetWClass" != c)
                    path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                else
                    path = await fileExplorer.Path(windowManager.CurrentWindow.Handle);
                if (shiftDown) commandRunner.RunWithCmd(swPaths.Terminal, true);
                else commandRunner.RunWithExplorer(swPaths.Terminal);
            });

            hotKeys.OpenCodeEditor.Event(async e =>
            {
                if (!windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog)
                {
                    commandRunner.RunWithExplorer(swPaths.Code);
                    return;
                }

                var paths = await fileExplorer.GetSelectedPath(windowManager.CurrentWindow.Handle);

                if (paths.Length == 0)
                {
                    var path = await fileExplorer.Path(windowManager.CurrentWindow.Handle);
                    commandRunner.RunWithExplorer(swPaths.Code, path);
                    return;
                }

                foreach (var path in paths)
                {
                    commandRunner.RunWithCmd(commandRunner.NormalizeCmd(swPaths.Code, path));
                }
            });
            hotKeys.WebSearch.Event(async e =>
            {
                e.Handled = true;

                var altDown = e.KeyboardState.IsDown(Keys.Menu);
                var url = altDown
                    ? swPaths.SearchEngineSecondary
                    : swPaths.SearchEngine;

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
            hotKeys.StartTaskExplorer.WithAliases(software.KeyAliases).Event(e =>
            {
                e.Handled = true;
                commandRunner.RunWithCmd(swPaths.ProcessExplorer);
            });

            hotKeys.OpenScreenRuler.WithAliases(software.KeyAliases).Event(e =>
            {
                e.Handled = true;
                commandRunner.RunWithCmd(swPaths.Ruler);
            });

            hotKeys.OpenScreenRuler.WithAliases(software.KeyAliases).Event(async e =>
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

                commandRunner.RunWithExplorer(swPaths.Inspect);
            });

            hotKeys.StartNotepad.WithAliases(software.KeyAliases).Event(async e =>
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

            hotKeys.StartVisualStudio.WithAliases(software.KeyAliases).Event(async e =>
            {
                if (!windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog) return;

                e.Handled = true;

                var path = await fileExplorer.Path(windowManager.CurrentWindow.Handle);
                if (string.IsNullOrEmpty(path))
                {
                    commandRunner.RunWithExplorer(swPaths.VisualStudio);
                    return;
                }

                Directory.CreateDirectory(path).EnumerateFiles("*.sln").Select(f => f.FullName).AsParallel().ForAll(s =>
                {
                    Process.Start(new ProcessStartInfo(swPaths.VisualStudio)
                        {UseShellExecute = true, Arguments = s, WorkingDirectory = path});
                });
            });

            hotKeys.StartGifRecord.WithAliases(software.KeyAliases).Event(e =>
            {
                e.Handled = true;
                commandRunner.RunWithCmd(swPaths.GifTool);
            });

            hotKeys.ToggleDictionary.MapOnHit(Shift + LAlt + D);
        }

    }
}