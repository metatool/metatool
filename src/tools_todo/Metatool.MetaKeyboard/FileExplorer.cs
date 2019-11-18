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

        public FileExplorer(IWindowManager windowManager, IFileExplorer fileExplorer, IConfig<Config> config)
        {
            _windowManager = windowManager;
            _fileExplorer  = fileExplorer;
            RegisterCommands();
            var hotKeys = config.CurrentValue.FileExplorerHotKeys;
            hotKeys.FocusItemsView.Register(e =>
            {
                var listBoxEle   = _windowManager.CurrentWindow?.FirstDescendant(c => c.ByClassName("UIItemsView"));
                var selectedItem = listBoxEle?.SelectedItems()?.FirstOrDefault();
                if (selectedItem != null) selectedItem.SetFocus();
                else listBoxEle.FirstChild(c => c.ByClassName("UIItem"))?.Select();
                e.Handled = true;
            }, _ => _windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

            hotKeys.FocusNavigationTreeView.Register(e =>
            {
                var winEle       = _windowManager.CurrentWindow?.FirstDescendant(cf => cf.ByClassName("SysTreeView32"));
                var selectedItem = winEle?.SelectedItems().FirstOrDefault();
                if (selectedItem != null)
                    selectedItem.SetFocus();
                else
                    winEle?.FirstDecendent(c => c.ByControlType(ControlType.TreeItem))?.Select();

                e.Handled = true;
            }, _ => _windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

            hotKeys.CopySelectedPath.Register(async e =>
            {
                var handle = _windowManager.CurrentWindow.Handle;
                var paths  = await _fileExplorer.GetSelectedPath(handle);
                var r      = string.Join(';', paths);
                System.Windows.Clipboard.SetText(r);
                e.Handled = true;
            }, _ => _windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

            hotKeys.NewFile.Register(async e =>
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
            }, _ => _windowManager.CurrentWindow.IsExplorer);

            hotKeys.ShowDesktopFolder.Register(e =>
            {
                _fileExplorer.Open(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                e.Handled = true;
            });
        }
    }
}