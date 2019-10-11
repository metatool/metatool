using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Metatool.Command;
using Metatool.Input;
using Metatool.Plugin;
using Metatool.UI;
using static Metatool.Input.Key;

namespace Metatool.MetaKeyboard
{
    public class FileExplorer
    {
        static bool IsExplorerOrDialog(IKeyEventArgs e)
        {
            var c = UI.Window.CurrentWindowClass;
            return "CabinetWClass" == c || "#32770" == c;
        }

        static bool IsExplorer(IKeyEventArgs e)
        {
            var c = UI.Window.CurrentWindowClass;
            return "CabinetWClass" == c;
        }

        public IKeyToken  FocusFileItemsView = (LWin + F).Down(e =>
        {
            using (var automation = new UIA3Automation())
            {
                var h = UI.Window.CurrentWindowHandle;
                var element = automation.FromHandle(h);
                var listBox = element.FindFirstDescendant(cf => cf.ByClassName("UIItemsView"))?.AsListBox();
                if (listBox?.SelectedItem != null) listBox.SelectedItem.Focus();
                else listBox?.Items.FirstOrDefault()?.Select();
            }

            e.Handled = true;
        }, IsExplorerOrDialog, "Focus &File Items View");


        public IKeyToken  FocusNavigationTreeView = (LWin + N).Down(e =>
        {
            using (var automation = new UIA3Automation())
            {
                var h = UI.Window.CurrentWindowHandle;
                var element = automation.FromHandle(h);
                var treeView = element.FindFirstDescendant(cf => cf.ByClassName("SysTreeView32"));
                treeView?.AsTree().SelectedTreeItem.Focus();
            }

            e.Handled = true;
        }, IsExplorerOrDialog, "Focus &Navigation Tree View");

        public IKeyToken  CopySelectedPath = (Caps + Pipe).Down(async e =>
        {
            var handle = UI.Window.CurrentWindowHandle;
            var paths = await Explorer.GetSelectedPath(handle);
            var r = string.Join(';', paths);
            System.Windows.Clipboard.SetText(r);
            e.Handled = true;
        }, IsExplorerOrDialog, "Copy Selected Files Path");


        public IKeyToken  NewFile = (Ctrl + Alt + N).Hit(async e =>
        {
            const string newFileName = "NewFile";
            var handle = UI.Window.CurrentWindowHandle;
            var fullPath = await Explorer.Path(handle);
            var fileName = newFileName;
            var i = 1;
            while (File.Exists(fullPath + "\\" + fileName))
            {
                fileName = newFileName + i++;
            }

            var file = File.Create(fullPath + "\\" + fileName);
            file.Close();
            var keyboard = ServiceLocator.GetService<IKeyboard>();
            Explorer.Select(handle, new[] { fileName });
            keyboard.Type(Keys.F2);
        }, IsExplorer, "&New File");

        public IKeyToken  ShowDesktopFolder = (LWin + D).Down(e =>
            {
                Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                e.Handled = true;
            }
            , null, "Show &Desktop Folder");
    }
}
