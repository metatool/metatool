using System;
using System.Diagnostics;
using System.Windows.Forms;
using Metatool.Input;
using Microsoft.Win32;
using static Metatool.Input.Key;

namespace Metatool.MetaKeyboard
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
            Keyboard.Default.ShowTip();
            e.Handled = true;
        }, null, "Show Tips");

        public IMetaKey DoublePinyinSwitch = (Pipe + P).Down(e =>
        {
            e.Handled = true;
            var keyName   = @"HKEY_CURRENT_USER\Software\Microsoft\InputMethod\Settings\CHS";
            var valueName = "Enable Double Pinyin";
            var k         = (int)Registry.GetValue(keyName, valueName, -1);
            if(k == 0)
                Registry.SetValue(keyName, valueName, 1);
            else if(k ==1)
                Registry.SetValue(keyName, valueName, 0);

        }, null, "&Toggle Double &Pinyin(Microsoft)");
    }
}
