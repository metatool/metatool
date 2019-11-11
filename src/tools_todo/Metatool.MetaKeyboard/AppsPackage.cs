﻿using System;
using Metatool.Input;
using Metatool.Utils;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using Metatool.Command;
using Metatool.Plugin;
using static Metatool.Input.Key;
using static Metatool.MetaKeyboard.KeyboardConfig;

namespace Metatool.MetaKeyboard
{
    public class Software : CommandPackage
    {
        public Software()
        {
            RegisterCommands();
        }
        public IKeyCommand  ToggleDictionary = (AK + D).MapOnHit(Shift + LAlt + D);

        public IKeyCommand  Find = (AK + F).Down(async e =>
        {
            e.Handled = true;
            var shiftDown = e.KeyboardState.IsDown(Keys.ShiftKey);

            var    c = Window.CurrentWindowClass;
            var arg = shiftDown
                ? "-newwindow"
                : "-toggle-window";
            
            if ("CabinetWClass" == c)
            {
                var path = await Explorer.Path(Window.CurrentWindowHandle);
                ProcessEx.Run(Config.Current.Tools.Everything, arg, "-path", path);
                return;
            }

            ProcessEx.Run(Config.Current.Tools.Everything, arg);
        }, null, "&Find With Everything");

        public IKeyCommand  OpenTerminal = (AK + T).Down(async e =>
        {
            string path;
            e.Handled = true;
            var c = Window.CurrentWindowClass;
            var shiftDown = e.KeyboardState.IsDown(Keys.ShiftKey);
            if ("CabinetWClass" != c)
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            else
                path = await Explorer.Path(Window.CurrentWindowHandle);
            if (shiftDown)
                ProcessEx.Run(Config.Current.Tools.Cmd, "/single", "/start", path);
            else
                ProcessEx.Run(Config.Current.Tools.Cmd, "/start", path);
        }, null, "Open &Terminal");

        public IKeyCommand  WebSearch = (AK + W).Down(async e =>
        {
            e.Handled = true;

            var altDown = e.KeyboardState.IsDown(Keys.Menu);
            var url = altDown
                ? Config.Current.Tools.SearchEngineSecondary
                : Config.Current.Tools.SearchEngine;

            var defaultPath = Browser.DefaultPath;
            var exeName     = Path.GetFileNameWithoutExtension(defaultPath);
            var process     = await VirtualDesktopManager.Inst.GetFirstProcessOnCurrentVirtualDesktop(exeName);
            if (process == null)
            {
                void RunAsNormalUser(string exewithArgs)
                {
                    var tempBat = Path.Combine(Path.GetTempPath(), "t.bat");
                    File.WriteAllText(tempBat, exewithArgs);

                    ProcessEx.Start(tempBat);
                }

                var IsElevated =
                    new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

                if (IsElevated)
                    RunAsNormalUser($"start \"\" \"{defaultPath}\" --new-window --new-instance \n exit");
                else
                {
                    new Process
                    {
                        StartInfo =
                        {
                            UseShellExecute = true,
                            FileName        = defaultPath,
                            ArgumentList    = {url, "--new-window", "--new-instance"},
                        }
                    }.Start();
                }

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
        }, null, "&Web Search(Alt: second)");


        private static readonly ICombination softwareTrigger = (AK + Space).Handled();

        public IKeyCommand  OpenScreenRuler = (softwareTrigger, R).Down(
            e =>
            {
                e.Handled = true;
                ProcessEx.Run(Config.Current.Tools.Ruler);
            }, null, "Screen &Ruler");

        public IKeyCommand  StartTaskExplorer = (softwareTrigger, T).Down(
            e =>
            {
                e.Handled = true;
                ProcessEx.Run(Config.Current.Tools.ProcessExplorer);
            }, null, "&Task Explorer ");

        public IKeyCommand  StartGifRecord = (softwareTrigger, G).Down(
            e =>
            {
                e.Handled = true;
                ProcessEx.Run(Config.Current.Tools.GifTool);
            }, null, "&Gif Record ");

        public IKeyCommand  StartNotepad = (softwareTrigger, N).Down(async e =>
        {
            e.Handled = true;
            var exeName = "Notepad";

            var notePads = await
                VirtualDesktopManager.Inst.GetProcessesOnCurrentVirtualDesktop(exeName,
                    p => p.MainWindowTitle == "Untitled - Notepad");


            var notePad = notePads.FirstOrDefault();

            var hWnd = notePad?.MainWindowHandle;

            if (hWnd != null)
            {
                Window.Show(hWnd.Value);
                return;
            }

            ProcessEx.Run("Notepad");
        }, null, "&Notepad");

        public IKeyCommand  StartVisualStudio = (softwareTrigger, V).Down(async e =>
        {
            if (!Window.IsExplorerOrOpenSaveDialog) return;

            e.Handled = true;

            var path = await Explorer.Path(Window.CurrentWindowHandle);
            if (string.IsNullOrEmpty(path))
            {
                ProcessEx.Start(Config.Current.Tools.VisualStudio);
                return;
            }

            Directory.CreateDirectory(path).EnumerateFiles("*.sln").Select(f => f.FullName).AsParallel().ForAll(s =>
            {
                Process.Start(new ProcessStartInfo(Config.Current.Tools.VisualStudio)
                    { UseShellExecute = true, Arguments = s, WorkingDirectory = path });
            });
        }, null, "&VisualStudio");

        public IKeyCommand  StartInspect = (softwareTrigger, I).Down(async e =>
        {
            var exeName = "Inspect";

            var processes = await
                VirtualDesktopManager.Inst.GetProcessesOnCurrentVirtualDesktop(exeName);

            var process = processes.FirstOrDefault();

            var hWnd = process?.MainWindowHandle;

            if (hWnd != null)
            {
                Window.Show(hWnd.Value);
                return;
            }


            ProcessEx.Start(Config.Current.Tools.Inspect);
        }, null, "&Inspect");

        public IKeyCommand  OpenCodeEditor = (AK + C).Hit(async e =>
        {
            e.Handled = true;
            if (!Window.IsExplorerOrOpenSaveDialog)
            {
                ProcessEx.Start(Config.Current.Tools.Code);
                return;
            }

            var paths = await Explorer.GetSelectedPath(Window.CurrentWindowHandle);

            if (paths.Length == 0)
            {
                ProcessEx.Start(Config.Current.Tools.Code);
                return;
            }

            foreach (var path in paths)
            {
                // need to find a way to start *.lnk with arguments, but still not killed if parent process exit
                var info = new ProcessStartInfo(Config.Current.Tools.Code) {UseShellExecute = true, Arguments = path};
                Process.Start(info);
            }
        }, null, "Open &Code Editor" );
    }
}
