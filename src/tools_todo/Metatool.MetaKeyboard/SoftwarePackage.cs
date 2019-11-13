using System;
using Metatool.Utils;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using Metatool.Command;
using Metatool.Service;
using Metatool.UI;
using Microsoft.Win32;
using static Metatool.Service.Key;
using static Metatool.MetaKeyboard.KeyboardConfig;

namespace Metatool.MetaKeyboard
{
    public sealed class Software : CommandPackage
    {

        private static ICommandRunner _commandRunner;
        private static INotify _notify;
       
        public Software(ICommandRunner commandRunner, INotify notify)
        {
            _commandRunner = commandRunner;
            _notify = notify;
            RegisterCommands();
        }
        public IKeyCommand DoublePinyinSwitch = (Pipe + P).Down(e =>
        {
            e.Handled = true;
            const string keyName = @"HKEY_CURRENT_USER\Software\Microsoft\InputMethod\Settings\CHS";
            const string valueName = "Enable Double Pinyin";
            var k         = (int)Registry.GetValue(keyName, valueName, -1);
            if (k == 0)
            {
                _notify.ShowMessage("Double Pinyin Enabled");
                Registry.SetValue(keyName, valueName, 1);
            }
            else if (k == 1)
            {
                _notify.ShowMessage("Full Pinyin Enabled");
                Registry.SetValue(keyName, valueName, 0);
            }
        }, null, "&Toggle Double &Pinyin(Microsoft)");

        public IKeyCommand ToggleDictionary = (AK + D).MapOnHit(Shift + LAlt + D);

        public IKeyCommand Find = (AK + F).Down(async e =>
        {
            e.Handled = true;
            var shiftDown = e.KeyboardState.IsDown(Shift);

            var c = Window.CurrentWindowClass;
            var arg = shiftDown
                ? "-newwindow"
                : "-toggle-window";

            if ("CabinetWClass" == c)
            {
                var path = await Explorer.Path(Window.CurrentWindowHandle);
                _commandRunner.RunWithCmd(_commandRunner.NormalizeCmd(Config.Current.Tools.Everything, arg, "-path", path));
                return;
            }

            _commandRunner.RunWithCmd(_commandRunner.NormalizeCmd(Config.Current.Tools.Everything, arg));
        }, null, "&Find With Everything");

        public IKeyCommand OpenTerminal = (AK + T).Down(async e =>
        {
            e.Handled = true;
            var shiftDown = e.KeyboardState.IsDown(Shift);
            string path;
            var    c = Window.CurrentWindowClass;
            if ("CabinetWClass" != c)
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            else
                path = await Explorer.Path(Window.CurrentWindowHandle);
            if(shiftDown) _commandRunner.RunWithCmd(Config.Current.Tools.Terminal, true);
            else _commandRunner.RunWithExplorer(Config.Current.Tools.Terminal);
        }, null, "Open &Terminal");

        public IKeyCommand WebSearch = (AK + W).Down(async e =>
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
                _commandRunner.RunAsNormalUser(defaultPath, url, "--new-window","--new-instance");
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

        public IKeyCommand OpenScreenRuler = (softwareTrigger, R).Down(
            e =>
            {
                e.Handled = true;
                _commandRunner.RunWithCmd(Config.Current.Tools.Ruler);
            }, null, "Screen &Ruler");

        public IKeyCommand StartTaskExplorer = (softwareTrigger, T).Down(
            e =>
            {
                e.Handled = true;
                _commandRunner.RunWithCmd(Config.Current.Tools.ProcessExplorer);
            }, null, "&Task Explorer ");

        public IKeyCommand StartGifRecord = (softwareTrigger, G).Down(
            e =>
            {
                e.Handled = true;
                _commandRunner.RunWithCmd(Config.Current.Tools.GifTool);
            }, null, "&Gif Record ");

        public IKeyCommand StartNotepad = (softwareTrigger, N).Down(async e =>
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

            _commandRunner.RunWithCmd("Notepad");
        }, null, "&Notepad");

        public IKeyCommand StartVisualStudio = (softwareTrigger, V).Down(async e =>
        {
            if (!Window.IsExplorerOrOpenSaveDialog) return;

            e.Handled = true;

            var path = await Explorer.Path(Window.CurrentWindowHandle);
            if (string.IsNullOrEmpty(path))
            {
                _commandRunner.RunWithExplorer(Config.Current.Tools.VisualStudio);
                return;
            }

            Directory.CreateDirectory(path).EnumerateFiles("*.sln").Select(f => f.FullName).AsParallel().ForAll(s =>
            {
                Process.Start(new ProcessStartInfo(Config.Current.Tools.VisualStudio)
                    {UseShellExecute = true, Arguments = s, WorkingDirectory = path});
            });
        }, null, "&VisualStudio");

        public IKeyCommand StartInspect = (softwareTrigger, I).Down(async e =>
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

            _commandRunner.RunWithExplorer(Config.Current.Tools.Inspect);
        }, null, "&Inspect");

        public IKeyCommand OpenCodeEditor = (AK + C).Handled().Hit(async e =>
        {
            if (!Window.IsExplorerOrOpenSaveDialog)
            {
                _commandRunner.RunWithExplorer(Config.Current.Tools.Code);
                return;
            }

            var paths = await Explorer.GetSelectedPath(Window.CurrentWindowHandle);

            if (paths.Length == 0)
            {
                _commandRunner.RunWithExplorer(Config.Current.Tools.Code);
                return;
            }

            foreach (var path in paths)
            {
                _commandRunner.RunWithCmd(_commandRunner.NormalizeCmd(Config.Current.Tools.Code, path));
            }
        }, null, "Open &Code Editor");
    }
}