using System;
using Metaseed.Input;
using Metaseed.UI;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static Metaseed.Input.Key;
using static Metaseed.MetaKeyboard.KeyboardConfig;

namespace Metaseed.MetaKeyboard
{
    public class Software : KeyMetaPackage
    {
        public IMetaKey ToggleDictionary = (AK + D).MapOnHit(Shift + LAlt + D);

        public IMetaKey Find = (AK + F).Down(async e =>
        {
            e.Handled = true;
            var shiftDown = e.KeyboardState.IsDown(Keys.ShiftKey);

            var c = Window.CurrentWindowClass;
            if ("CabinetWClass" != c)
            {
                Utils.Run(shiftDown
                    ? $"{Config.Current.Tools.EveryThing} -newwindow"
                    : $"{Config.Current.Tools.EveryThing} -toggle-window");
                return;
            }

            var path = await Explorer.Path(Window.CurrentWindowHandle);
            Utils.Run(shiftDown
                ? $"{Config.Current.Tools.EveryThing} -path {path} -newwindow"
                : $"{Config.Current.Tools.EveryThing} -path {path} -toggle-window");
        }, null, "&Find With Everything");

        public IMetaKey OpenTerminal = (AK + T).Down(async e =>
        {
            e.Handled = true;
            var shiftDown = e.KeyboardState.IsDown(Keys.ShiftKey);

            var c = Window.CurrentWindowClass;
            if ("CabinetWClass" != c)
            {
                var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                Utils.Run(shiftDown
                    ? $"{Config.Current.Tools.Cmd}  /single /start \"{folderPath}\""
                    : $"{Config.Current.Tools.Cmd}  /start \"{folderPath}\"");
                return;
            }

            var path = await Explorer.Path(Window.CurrentWindowHandle);
            Utils.Run(shiftDown
                ? $"{Config.Current.Tools.Cmd} /start \"{path}\""
                : $"{Config.Current.Tools.Cmd} /single /start \"{path}\"");
        }, null, "Open &Terminal");

        public IMetaKey WebSearch = (AK + W).Down(async e =>
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
                new Process
                {
                    StartInfo =
                    {
                        // below 2 lines are used to load extensions for chrome if not started yet. if the metakey is run as admin.
                        UseShellExecute = false,
                        LoadUserProfile = true,
                        FileName        = defaultPath,
                        ArgumentList    = {url, "--new-window", "--new-instance"},
                    }
                }.Start();
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

        public IMetaKey OpenScreenRuler = (softwareTrigger, R).Down(
            e =>
            {
                e.Handled = true;
                Utils.Run(Config.Current.Tools.Ruler);
            }, null, "Screen &Ruler");

        public IMetaKey StartTaskExplorer = (softwareTrigger, T).Down(
            e =>
            {
                e.Handled = true;
                Utils.Run(Config.Current.Tools.ProcessExplorer);
            }, null, "&Task Explorer ");

        public IMetaKey StartGifRecord = (softwareTrigger, G).Down(
            e =>
            {
                e.Handled = true;
                Utils.Run(Config.Current.Tools.GifTool);
            }, null, "&Gif Record ");

        public IMetaKey StartNotepad = (softwareTrigger, N).Down(async e =>
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

            Utils.Run("Notepad");
        }, null, "&Notepad");

        public IMetaKey StartVisualStudio = (softwareTrigger, V).Down(async e =>
        {
            if (!Window.IsExplorerOrOpenSaveDialog) return;

            e.Handled = true;

            var path = await Explorer.Path(Window.CurrentWindowHandle);
            if (string.IsNullOrEmpty(path))
            {
                Process.Start(new ProcessStartInfo(Config.Current.Tools.VisualStudio)
                    {UseShellExecute = true});
                return;
            }

            Directory.CreateDirectory(path).EnumerateFiles("*.sln").Select(f => f.FullName).AsParallel().ForAll(s =>
            {
                Process.Start(new ProcessStartInfo(Config.Current.Tools.VisualStudio)
                    {UseShellExecute = true, Arguments = s, WorkingDirectory = path});
            });
        }, null, "&VisualStudio");

        public IMetaKey StartInspect = (softwareTrigger, I).Down(async e =>
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

            var info = new ProcessStartInfo(Config.Current.Tools.Inspect) {UseShellExecute = true};

            Process.Start(info);
        }, null, "&Inspect");

        public IMetaKey OpenCodeEditor = (AK + C).Hit(async e =>
        {
            var info = new ProcessStartInfo(Config.Current.Tools.Code) {UseShellExecute = true};

            if (!Window.IsExplorerOrOpenSaveDialog)
            {
                Process.Start(info);
                return;
            }

            var paths = await Explorer.GetSelectedPath(Window.CurrentWindowHandle);

            if (paths.Length == 0)
            {
                Process.Start(info);
                return;
            }

            foreach (var path in paths)
            {
                info.Arguments = path;
                Process.Start(info);
            }
        }, null, "Open &Code Editor", true);
    }
}