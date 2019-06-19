using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Metaseed.Input;
using Metaseed.UI;

namespace Metaseed.MetaKeyboard
{
    public class FunctionalKeys
    {
        public FunctionalKeys()
        {
            Keys.F.With(Keys.LWin).Down("Metaseed.FocusFileItemsView", "Focus &File Items View", e =>
            {
                var c = UI.Window.CurrentWindowClass;
                if ("CabinetWClass" != c && "#32770" != c) return;// Windows Explorer or open/save as dialog

                using (var automation = new UIA3Automation())
                {
                    var h = UI.Window.CurrentWindowHandle;
                    var element = automation.FromHandle(h);
                    var listBox = element.FindFirstDescendant(cf => cf.ByClassName("UIItemsView"))?.AsListBox();
                    if (listBox?.SelectedItem != null) listBox.SelectedItem.Focus();
                    else listBox?.Items.FirstOrDefault()?.Select();
                }

                e.Handled = true;
            });


            Keys.N.With(Keys.LWin).Down("Metaseed.FocusNavigAtionTreeView", "Focus &Navigation Tree View", e =>
            {
                var c = UI.Window.CurrentWindowClass;
                if ("CabinetWClass" != c && "#32770" != c) return;

                using (var automation = new UIA3Automation())
                {
                    var h = UI.Window.CurrentWindowHandle;
                    var element = automation.FromHandle(h);
                    var treeView = element.FindFirstDescendant(cf => cf.ByClassName("SysTreeView32"));
                    treeView?.AsTree().SelectedTreeItem.Focus();
                }

                e.Handled = true;
            });

            Keys.OemPipe.With(Keys.CapsLock).Down("Metaseed.CopySelectedFilesPath", "Copy Selected Files Path", async e =>
            {
                var c = UI.Window.CurrentWindowClass;
                if ("CabinetWClass" != c && "#32770" != c) return;

                var handle = UI.Window.CurrentWindowHandle;
                var paths = await Explorer.GetSelectedPath(handle);
                var r = string.Join(';', paths);
                Clipboard.SetText(r);
                e.Handled = true;
            });

            Keys.D.With(Keys.LWin).Down("Metaseed.ShowDesktopFolder", "Show &Desktop Folder", e =>
           {
               Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
               e.Handled = true;
           });

            Keys.N.With(Keys.Control).With(Keys.Alt).Hit("Metaseed.NewFile", "&New File", async e =>
             {
                 const string newFileName = "NewFile";
                 var handle = UI.Window.CurrentWindowHandle;
                 var fullPath = await Explorer.Path(handle);
                 var fileName = newFileName;
                 int i = 1;
                 while (File.Exists(fullPath + "\\" + fileName))
                 {
                     fileName = newFileName + i++;
                 }
                 var file = File.Create(fullPath + "\\" + fileName);
                 file.Close();
                 Explorer.Select(handle, new[] { fileName });
                 Keyboard.Type(Keys.F2);

             }, e =>
             {
                 var c = UI.Window.CurrentWindowClass;
                 if ("CabinetWClass" != c) return false;
                 return true;
             });

            Keys.C.With(Keys.LControlKey).With(Keys.LWin).With(Keys.LMenu)
                .Down("Metaseed.Close_MetaKeyBoard", "Close", e =>
                {
                    Environment.Exit(0);
                });


            Keys.X.With(Keys.LControlKey).With(Keys.LWin).With(Keys.LMenu)
                .Down("Metaseed.Restart_MetaKeyBoard", "Restart", e =>
                {
                    var p = Application.ExecutablePath;
                    var path = p.Remove(p.Length - 4, 4) + ".exe";
                    Process.Start(path);
                    Environment.Exit(0);
                });
            Keys.F1.Down("Metaseed.Help", "Show Tips", e => { Keyboard.ShowTip(); });
        }

    }
}
