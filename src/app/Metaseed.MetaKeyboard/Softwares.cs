using System;
using Metaseed.Input;
using Metaseed.UI;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Metaseed.MetaKeyboard
{
    public class Utilities
    {
        public Utilities()
        {
            Keys.C.With(Keys.CapsLock).Hit("Metaseed.OpenCodeEditor", "Open &Code Editor", async e =>
            {
                var code = Config.Inst.Tools.Code;
                var info = new ProcessStartInfo(code) {UseShellExecute = true};

                var c = Window.CurrentWindowClass;
                if ("CabinetWClass" != c)
                {
                    Process.Start(info);
                    return;
                }

                var handle = Window.CurrentWindowHandle;
                var paths = await Explorer.GetSelectedPath(handle);

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
            }, null, true);

            Keys.D.With(Keys.CapsLock).MapOnHit(Keys.D.With(Keys.LMenu).With(Keys.ShiftKey));

            Keys.F.With(Keys.CapsLock).Down("Metaseed.Find", "&Find With Everything", async e =>
            {
                e.Handled = true;
                var shiftDown = e.KeyboardState.IsDown(Keys.ShiftKey);

                var c = Window.CurrentWindowClass;
                if ("CabinetWClass" != c)
                {
                    Utils.Run(shiftDown
                        ? $"{Config.Inst.Tools.EveryThing} -newwindow"
                        : $"{Config.Inst.Tools.EveryThing} -toggle-window");
                    return;
                }

                var path = await Explorer.Path(Window.CurrentWindowHandle);
                Utils.Run(shiftDown
                    ? $"{Config.Inst.Tools.EveryThing} -path {path} -newwindow"
                    : $"{Config.Inst.Tools.EveryThing} -path {path} -toggle-window");
            });
            
            Keys.T.With(Keys.CapsLock).Down("Metaseed.Terminal", "Open &Terminal", async e =>
            {
                e.Handled = true;
                var shiftDown = e.KeyboardState.IsDown(Keys.ShiftKey);

                var c = Window.CurrentWindowClass;
                if ("CabinetWClass" != c)
                {
                    var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    Utils.Run(shiftDown
                        ? $"{Config.Inst.Tools.Cmd}  /single /start \"{folderPath}\""
                        : $"{Config.Inst.Tools.Cmd}  /start \"{folderPath}\"");
                    return;
                }

                var path = await Explorer.Path(Window.CurrentWindowHandle);
                Utils.Run(shiftDown
                    ? $"{Config.Inst.Tools.Cmd} /start \"{path}\""
                    : $"{Config.Inst.Tools.Cmd} /single /start \"{path}\"");
            });
            Keys.W.With(Keys.CapsLock).Down("Metaseed.WebSearch", "&Web Search", e =>
            {
                var altDown = e.KeyboardState.IsDown(Keys.Menu);
                new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = true,
                        FileName = altDown
                            ? Config.Inst.Tools.SearchEngineSecondary
                            : Config.Inst.Tools.SearchEngine
                    }
                }.Start();
            });
            var softwareTrigger = Keys.Space.With(Keys.CapsLock);
            softwareTrigger.Then(Keys.R).Down("Metaseed.ScreenRuler", "Start Screen &Ruler",
                e => { Utils.Run(Config.Inst.Tools.Ruler); });

            softwareTrigger.Then(Keys.T).Down("Metaseed.TaskExplorer", "Start &Task Explorer ",
                e => { Utils.Run(Config.Inst.Tools.ProcessExplorer); });

            softwareTrigger.Then(Keys.G).Down("Metaseed.GifRecord", "Start &Gif Record ",
                e => { Utils.Run(Config.Inst.Tools.GifTool); });
            
            softwareTrigger.Then(Keys.N).Down("Metaseed.NodePad", "Start &Notepad ", e =>
            {
                var exeName = "Notepad";
                var notePad = Process.GetProcessesByName(exeName);

                var hWnd = notePad.FirstOrDefault(p => p.MainWindowTitle == "Untitled - Notepad")?.MainWindowHandle;
                if (hWnd != null)
                {
                    Window.Show(hWnd.Value);
                    return;
                }

                Utils.Run("Notepad");
            });


        }
    }
}