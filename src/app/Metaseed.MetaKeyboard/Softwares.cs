using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using FlaUI.UIA3;
using Metaseed.UI;
using Metaseed.Input;

namespace Metaseed.MetaKeyboard
{
    public class Utilities
    {
        public Utilities()
        {
            Keys.C.With(Keys.CapsLock).Hit("Metaseed.OpenCodeEditor", "Open &Code Editor", async e =>
             {
                 var code = Config.Inst.Tools.Code;
                 var info = new ProcessStartInfo(code) { UseShellExecute = true };

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

            var softwareTrigger = Keys.Space.With(Keys.CapsLock);
            softwareTrigger.Then(Keys.R).Down("Metaseed.ScreenRuler", "Start Screen &Ruler", () =>
             {
                 Utils.Run(Config.Inst.Tools.Ruler);
             });

            softwareTrigger.Then(Keys.T).Down("Metaseed.TaskExplorer", "Start &Task Explorer ", () =>
             {
                 Utils.Run(Config.Inst.Tools.ProcessExplorer);
             });

            softwareTrigger.Then(Keys.G).Down("Metaseed.GifRecord", "Start &Gif Record ", () =>
             {
                 Utils.Run(Config.Inst.Tools.GifTool);
             });

        }
    }
}
