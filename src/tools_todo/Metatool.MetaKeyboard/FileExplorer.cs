using System;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Forms;
using Metatool.Service;
using static Metatool.Service.Key;

namespace Metatool.MetaKeyboard
{
    public class FileExplorer : CommandPackage
    {
        private static IWindowManager _windowManager;
        private static IFileExplorer  _fileExplorer;

        public FileExplorer(IWindowManager windowManager, IFileExplorer fileExplorer)
        {
            _windowManager = windowManager;
            _fileExplorer  = fileExplorer;
            RegisterCommands();
        }

        public IKeyCommand FocusFileItemsView = (LWin + F).Down(e =>
        {
            var listBoxEle   = _windowManager.CurrentWindow?.FirstDescendant(c => c.ByClassName("UIItemsView"));
            var selectedItem = listBoxEle?.SelectedItems()?.FirstOrDefault();
            if (selectedItem != null) selectedItem.SetFocus();
            else listBoxEle.FirstChild(c => c.ByClassName("UIItem"))?.Select();
            e.Handled = true;
        }, _ => _windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog, "Focus &File Items View");


        public IKeyCommand FocusNavigationTreeView = (LWin + N).Down(e =>
        {
            var winEle       = _windowManager.CurrentWindow?.FirstDescendant(cf => cf.ByClassName("SysTreeView32"));
            var selectedItem = winEle?.SelectedItems().FirstOrDefault();
            if (selectedItem != null)
                selectedItem.SetFocus();
            else
                winEle?.FirstDecendent(c => c.ByControlType(ControlType.TreeItem))?.Select();

            e.Handled = true;
        }, _ => _windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog, "Focus &Navigation Tree View");

        public IKeyCommand CopySelectedPath = (Caps + Pipe).Down(async e =>
        {
            var handle = _windowManager.CurrentWindow.Handle;
            var paths  = await _fileExplorer.GetSelectedPath(handle);
            var r      = string.Join(';', paths);
            System.Windows.Clipboard.SetText(r);
            e.Handled = true;
        }, _ => _windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog, "Copy Selected Files Path");


        public IKeyCommand NewFile = (Ctrl + Alt + N).Hit(async e =>
        {
            e.Handled = true;
            const string newFileName = "NewFile";
            var          handle      = _windowManager.CurrentWindow.Handle;
            var          fullPath    = await _fileExplorer.Path(handle);
            var          fileName    = newFileName;
            var          i           = 1;
            while (File.Exists(fullPath + "\\" + fileName))
            {
                fileName = newFileName + i++;
            }

            var file = File.Create(fullPath + "\\" + fileName);
            file.Close();
            var keyboard = Services.Get<IKeyboard>();
            _fileExplorer.Select(handle, new[] {fileName});
            keyboard.Type(Keys.F2);
        }, _ => _windowManager.CurrentWindow.IsExplorer, "&New File");

        public IKeyCommand ShowDesktopFolder = (LWin + D).Down(e =>
        {
            _fileExplorer.Open(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            e.Handled = true;
        }, null, "Show &Desktop Folder");
    }
}