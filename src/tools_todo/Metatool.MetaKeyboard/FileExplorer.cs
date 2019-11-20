using System;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Forms;
using Metatool.Service;

namespace Metatool.MetaKeyboard
{
    public class FileExplorer : CommandPackage
    {

        public FileExplorer(IWindowManager windowManager, IFileExplorer fileExplorer, IConfig<Config> config)
        {
            RegisterCommands();
            var hotKeys = config.CurrentValue.FileExplorerPackage.HotKeys;
            hotKeys.FocusItemsView.Event(e =>
            {
                var listBoxEle   = windowManager.CurrentWindow?.FirstDescendant(c => c.ByClassName("UIItemsView"));
                var selectedItem = listBoxEle?.SelectedItems()?.FirstOrDefault();
                if (selectedItem != null) selectedItem.SetFocus();
                else listBoxEle.FirstChild(c => c.ByClassName("UIItem"))?.Select();
                e.Handled = true;
            }, _ => windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

            hotKeys.FocusNavigationTreeView.Event(e =>
            {
                var winEle       = windowManager.CurrentWindow?.FirstDescendant(cf => cf.ByClassName("SysTreeView32"));
                var selectedItem = winEle?.SelectedItems().FirstOrDefault();
                if (selectedItem != null)
                    selectedItem.SetFocus();
                else
                    winEle?.FirstDecendent(c => c.ByControlType(ControlType.TreeItem))?.Select();

                e.Handled = true;
            }, _ => windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

            hotKeys.CopySelectedPath.Event(async e =>
            {
                var handle = windowManager.CurrentWindow.Handle;
                var paths  = await fileExplorer.GetSelectedPath(handle);
                var r      = string.Join(';', paths);
                System.Windows.Clipboard.SetText(r);
                e.Handled = true;
            }, _ => windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

            hotKeys.NewFile.Event(async e =>
            {
                e.Handled = true;
                const string newFileName = "NewFile";
                var          handle      = windowManager.CurrentWindow.Handle;
                var          fullPath    = await fileExplorer.Path(handle);
                var          fileName    = newFileName;
                var          i           = 1;
                while (File.Exists(fullPath + "\\" + fileName))
                {
                    fileName = newFileName + i++;
                }

                var file = File.Create(fullPath + "\\" + fileName);
                file.Close();
                var keyboard = Services.Get<IKeyboard>();
                fileExplorer.Select(handle, new[] {fileName});
                keyboard.Type(Keys.F2);
            }, _ => windowManager.CurrentWindow.IsExplorer);

            hotKeys.ShowDesktopFolder.Event(e =>
            {
                fileExplorer.Open(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                e.Handled = true;
            });
        }
    }
}