using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Metaseed.Input;
using Metaseed.UI;

namespace ConsoleApp1
{
    public class FunctionalKeys
    {
        public FunctionalKeys()
        {
            Keys.F.With(Keys.LWin).Down("Metaseed.FocusFileItemsView", "Focus &File Items View", e =>
            {
                var c = UI.CurrentWindowClass;
                if ("CabinetWClass" != c && "#32770" != c) return;// Windows Explorer or open/save as dialog

                using (var automation = new UIA3Automation())
                {
                    var h = UI.CurrentWindowHandle;
                    var element = automation.FromHandle(h);
                    var listBox = element.FindFirstDescendant(cf => cf.ByClassName("UIItemsView"))?.AsListBox();
                    if (listBox?.SelectedItem != null) listBox.SelectedItem.Focus();
                    else listBox?.Items?[0].Select();
                }

                e.Handled = true;
            });


            Keys.N.With(Keys.LWin).Down("Metaseed.FocusNavigAtionTreeView", "Focus &Navigation Tree View", e =>
            {
                var c = UI.CurrentWindowClass;
                if ("CabinetWClass" != c && "#32770" != c) return;

                using (var automation = new UIA3Automation())
                {
                    var h = UI.CurrentWindowHandle;
                    var element = automation.FromHandle(h);
                    var treeView = element.FindFirstDescendant(cf => cf.ByClassName("SysTreeView32"));
                    treeView?.AsTree().SelectedTreeItem.Focus();
                }

                e.Handled = true;
            });

            Keys.OemPipe.With(Keys.CapsLock).Down("Metaseed.CopySelectedFilesPath", "Copy Selected Files Path", e =>
            {
                var c = UI.CurrentWindowClass;
                if ("CabinetWClass" != c && "#32770" != c) return;

                UI.Dispatch(() =>
                {
                    IntPtr handle = UI.CurrentWindowHandle;
                    var paths = Explorer.GetSelectedFilePath(handle);
                    var r = string.Join(';', paths);
                    Clipboard.SetText(r);
                });
                e.Handled = true;
            });

            Keys.D.With(Keys.LWin).Down("Metaseed.ShowDesktopFolder", "Show &Desktop Folder", e =>
           {
               Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
               e.Handled = true;
           });
        }

    }
}
