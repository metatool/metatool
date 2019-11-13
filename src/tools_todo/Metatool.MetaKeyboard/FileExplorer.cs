using System;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Forms;
using Metatool.Command;
using Metatool.Service;
using Metatool.Utils;
using static Metatool.Service.Key;

namespace Metatool.MetaKeyboard
{
    public class FileExplorer : CommandPackage
    {
        private static IWindowManager WindowManager;
        public FileExplorer(IWindowManager windowManager)
        {
            WindowManager = windowManager;
            RegisterCommands();
        }

        public IKeyCommand FocusFileItemsView = (LWin + F).Down(e =>
        {
            var listBoxEle   = WindowManager.CurrentWindow?.FirstDescendant(c => c.ByClassName("UIItemsView"));
            var selectedItem = listBoxEle.SelectedItems()?.FirstOrDefault();
            if (selectedItem != null) selectedItem.SetFocus();
            else listBoxEle.FirstChild(c => c.ByClassName("UIItem"))?.Select();
            e.Handled = true;
        }, _ => WindowManager.CurrentWindow.IsExplorerOrOpenSaveDialog, "Focus &File Items View");


        public IKeyCommand FocusNavigationTreeView = (LWin + N).Down(e =>
        {
            var winEle       = WindowManager.CurrentWindow?.FirstDescendant(cf => cf.ByClassName("SysTreeView32"));
            var selectedItem = winEle?.SelectedItems().FirstOrDefault();
            if (selectedItem != null)
                selectedItem.SetFocus();
            else
                winEle?.FirstDecendent(c => c.ByControlType(ControlType.TreeItem))?.Select();

            e.Handled = true;
        }, _ => WindowManager.CurrentWindow.IsExplorerOrOpenSaveDialog, "Focus &Navigation Tree View");

        public IKeyCommand CopySelectedPath = (Caps + Pipe).Down(async e =>
        {
            var handle = WindowManager.CurrentWindow.Handle;
            var paths  = await Explorer.GetSelectedPath(handle);
            var r      = string.Join(';', paths);
            System.Windows.Clipboard.SetText(r);
            e.Handled = true;
        }, _ => WindowManager.CurrentWindow.IsExplorerOrOpenSaveDialog, "Copy Selected Files Path");


        public IKeyCommand NewFile = (Ctrl + Alt + N).Hit(async e =>
        {
            const string newFileName = "NewFile";
            var          handle      = WindowManager.CurrentWindow.Handle;
            var          fullPath    = await Explorer.Path(handle);
            var          fileName    = newFileName;
            var          i           = 1;
            while (File.Exists(fullPath + "\\" + fileName))
            {
                fileName = newFileName + i++;
            }

            var file = File.Create(fullPath + "\\" + fileName);
            file.Close();
            var keyboard = Services.Get<IKeyboard>();
            Explorer.Select(handle, new[] {fileName});
            keyboard.Type(Keys.F2);
        }, _ => WindowManager.CurrentWindow.IsExplorer, "&New File");

        public IKeyCommand ShowDesktopFolder = (LWin + D).Down(e =>
        {
            Explorer.Open(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            e.Handled = true;
        }, null, "Show &Desktop Folder");
    }
}