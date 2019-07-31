using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Metaseed.Input;
using Metaseed.UI;
using static Metaseed.Input.Key;

namespace Metaseed.MetaKeyboard
{
    public class FunctionalKeys
    {
        public FunctionalKeys()
        {
            (LWin + F).Down(e =>
            {
                var c = UI.Window.CurrentWindowClass;
                if ("CabinetWClass" != c && "#32770" != c) return; // Windows Explorer or open/save as dialog

                using (var automation = new UIA3Automation())
                {
                    var h       = UI.Window.CurrentWindowHandle;
                    var element = automation.FromHandle(h);
                    var listBox = element.FindFirstDescendant(cf => cf.ByClassName("UIItemsView"))?.AsListBox();
                    if (listBox?.SelectedItem != null) listBox.SelectedItem.Focus();
                    else listBox?.Items.FirstOrDefault()?.Select();
                }

                e.Handled = true;
            }, "Metaseed.FocusFileItemsView", "Focus &File Items View");


            (LWin + N).Down(e =>
            {
                var c = UI.Window.CurrentWindowClass;
                if ("CabinetWClass" != c && "#32770" != c) return;

                using (var automation = new UIA3Automation())
                {
                    var h        = UI.Window.CurrentWindowHandle;
                    var element  = automation.FromHandle(h);
                    var treeView = element.FindFirstDescendant(cf => cf.ByClassName("SysTreeView32"));
                    treeView?.AsTree().SelectedTreeItem.Focus();
                }

                e.Handled = true;
            }, "Metaseed.FocusNavigationTreeView", "Focus &Navigation Tree View");

            (Caps + Pipe).Down(async e =>
            {
                var c = UI.Window.CurrentWindowClass;
                if ("CabinetWClass" != c && "#32770" != c) return;

                var handle = UI.Window.CurrentWindowHandle;
                var paths  = await Explorer.GetSelectedPath(handle);
                var r      = string.Join(';', paths);
                System.Windows.Clipboard.SetText(r);
                e.Handled = true;
            }, "Metaseed.CopySelectedFilesPath", "Copy Selected Files Path");

            (LWin + D).Down(e =>
                {
                    Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                    e.Handled = true;
                }
                , "Metaseed.ShowDesktopFolder", "Show &Desktop Folder");

            (Ctrl + Alt + N).Hit(async e =>
            {
                const string newFileName = "NewFile";
                var          handle      = UI.Window.CurrentWindowHandle;
                var          fullPath    = await Explorer.Path(handle);
                var          fileName    = newFileName;
                var          i           = 1;
                while (File.Exists(fullPath + "\\" + fileName))
                {
                    fileName = newFileName + i++;
                }

                var file = File.Create(fullPath + "\\" + fileName);
                file.Close();
                Explorer.Select(handle, new[] {fileName});
                Keyboard.Type(Keys.F2);
            }, e =>
            {
                var c = UI.Window.CurrentWindowClass;
                if ("CabinetWClass" != c) return false;
                return true;
            }, "Metaseed.NewFile", "&New File");

            (LCtrl + LWin + C).With(Keys.LMenu)
                .Down(e =>
                {
                    Notify.ShowMessage("MetaKeyBoard Closing...");
                    Environment.Exit(0);
                }, "Metaseed.Close_MetaKeyBoard", "Close");

            (LCtrl + LWin + LAlt + X).Down(e =>
            {
                Notify.ShowMessage("MetaKeyBoard Restarting...");
                var p    = Application.ExecutablePath;
                var path = p.Remove(p.Length - 4, 4) + ".exe";
                Process.Start(path);
                Environment.Exit(0);
            }, "Metaseed.Restart_MetaKeyBoard", "Restart");

            (Caps + Question).Down(e =>
            {
                Keyboard.ShowTip();
                e.Handled = true;
            }, "Metaseed.Help", "Show Tips");
        }
    }
}