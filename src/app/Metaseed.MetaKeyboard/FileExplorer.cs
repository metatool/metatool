using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Metaseed.Input;
using Metaseed.UI;
using static Metaseed.Input.Key;

namespace Metaseed.MetaKeyboard
{
    public class FileExplorer
    {
        static bool IsExplorerOrDialog(KeyEventArgsExt e)
        {
            var c = UI.Window.CurrentWindowClass;
            return "CabinetWClass" == c || "#32770" == c;
        }

        static bool IsExplorer(KeyEventArgsExt e)
        {
            var c = UI.Window.CurrentWindowClass;
            return "CabinetWClass" == c;
        }

        public IMetaKey FocusFileItemsView = (LWin + F).Down(e =>
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


        public IMetaKey FocusNavigationTreeView = (LWin + N).Down(e =>
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

        public IMetaKey CopySelectedPath = (Caps + Pipe).Down(async e =>
        {
            var handle = UI.Window.CurrentWindowHandle;
            var paths = await Explorer.GetSelectedPath(handle);
            var r = string.Join(';', paths);
            System.Windows.Clipboard.SetText(r);
            e.Handled = true;
        }, IsExplorerOrDialog, "Copy Selected Files Path");


        public IMetaKey NewFile = (Ctrl + Alt + N).Hit(async e =>
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
            Explorer.Select(handle, new[] { fileName });
            Keyboard.Type(Keys.F2);
        }, IsExplorer, "&New File");

        public IMetaKey ShowDesktopFolder = (LWin + D).Down(e =>
            {
                Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                e.Handled = true;
            }
            , null, "Show &Desktop Folder");
    }
}
