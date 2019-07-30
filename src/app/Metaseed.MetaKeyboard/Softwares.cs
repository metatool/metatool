using System;
using Metaseed.Input;
using Metaseed.UI;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static Metaseed.Input.Key;

namespace Metaseed.MetaKeyboard
{
    public class Utilities
    {
        public Utilities()
        {
            (Caps + C).Hit(async e =>
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
            }, null, "Metaseed.OpenCodeEditor", "Open &Code Editor", true);

            (Caps + Q).MapOnHit(Keys.D.With(Keys.LMenu).With(Keys.ShiftKey));

            (Caps + F).Down(async e =>
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
            }, "Metaseed.Find", "&Find With Everything");

            (Caps + T).Down(async e =>
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
            }, "Metaseed.Terminal", "Open &Terminal");

            (Caps + W).Down(async e =>
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
                            UseShellExecute = true,
                            FileName        = defaultPath,
                            ArgumentList    = {url, "--new-window", "--new-instance"}
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
            }, "Metaseed.WebSearch", "&Web Search(Alt: second)");


            var softwareTrigger = (Caps + Space).Handled();

            (softwareTrigger, R).Down(
                e =>
                {
                    e.Handled = true;
                    Utils.Run(Config.Current.Tools.Ruler);
                }, "Metaseed.ScreenRuler", "Start Screen &Ruler");

            softwareTrigger.Then(Keys.T).Down(
                e =>
                {
                    e.Handled = true;
                    Utils.Run(Config.Current.Tools.ProcessExplorer);
                }, "Metaseed.TaskExplorer", "Start &Task Explorer ");

            softwareTrigger.Then(Keys.G).Down(
                e =>
                {
                    e.Handled = true;
                    Utils.Run(Config.Current.Tools.GifTool);
                }, "Metaseed.GifRecord", "Start &Gif Record ");

            softwareTrigger.Then(Keys.N).Down(async e =>
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
            }, "Metaseed.NodePad", "Start &Notepad ");

            softwareTrigger.Then(Keys.V).Down(async e =>
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
            }, "Metaseed.VisualStudio", "Start &VisualStudio ");
        }
    }
}