using System;
using System.Diagnostics;
using System.Windows.Forms;
using Metaseed.Input;
using static Metaseed.Input.Key;

namespace Metaseed.MetaKeyboard
{
    public class FunctionalKeys : KeyMetaPackage
    {
        public IMetaKey CloseMetaKeys = (LWin + L).Handled().Down(null);



        public IMetaKey CloseMetaKey = (LCtrl + LWin + C).With(Keys.LMenu)
            .Down(e =>
            {
                Notify.ShowMessage("MetaKeyBoard Closing...");
                Environment.Exit(0);
            }, null, "Close");

        public IMetaKey RestartMetakeyAdmin = (LCtrl + LWin + LAlt + X).Down(e =>
        {
            Notify.ShowMessage("MetaKeyBoard Restarting...");
            var p    = Application.ExecutablePath;
            var path = p.Remove(p.Length - 4, 4) + ".exe";

            new Process()
            {
                StartInfo =
                {
                    FileName        = path,
                    Verb            = "runas",
                    UseShellExecute = true
                }
            }.Start();
            Environment.Exit(0);
        }, null, "Restart(Admin)");

        public IMetaKey ShowTips = (Caps + Question).Down(e =>
        {
            Keyboard.ShowTip();
            e.Handled = true;
        }, null, "Show Tips");
    }
}