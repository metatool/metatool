using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Metatool.Service;
using static Metatool.Service.Key;

namespace Metatool.MetaKeyboard
{
    public class FileExplorer : CommandPackage
    {

        public FileExplorer(IWindowManager windowManager, IFileExplorer fileExplorer, IConfig<Config> config, INotify notify, IKeyboard keyboard)
        {
            RegisterCommands();
            var hotKeys = config.CurrentValue.FileExplorerPackage.Hotkeys;
            hotKeys.FocusItemsView.OnEvent(e =>
            {
                var listBoxEle   = windowManager.CurrentWindow?.FirstDescendant(c => c.ByClassName("UIItemsView"));
                var selectedItem = listBoxEle?.SelectedItems()?.FirstOrDefault();
                if (selectedItem != null) selectedItem.SetFocus();
                else listBoxEle.FirstChild(c => c.ByClassName("UIItem"))?.Select();
                e.Handled = true;
            }, _ => windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

            hotKeys.FocusNavigationTreeView.OnEvent(e =>
            {
                var winEle       = windowManager.CurrentWindow?.FirstDescendant(cf => cf.ByClassName("SysTreeView32"));
                var selectedItem = winEle?.SelectedItems().FirstOrDefault();
                if (selectedItem != null)
                    selectedItem.SetFocus();
                else
                    winEle?.FirstDecendent(c => c.ByControlType(ControlType.TreeItem))?.Select();

                e.Handled = true;
            }, _ => windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

            hotKeys.CopySelectedPath.OnEvent(async e =>
            {
                e.Handled = true;
                var handle = windowManager.CurrentWindow.Handle;
                var paths  = await fileExplorer.GetSelectedPaths(handle);
                var r      = string.Join(';', paths);
                notify.ShowMessage($"Path Copied: {r}");
                System.Windows.Clipboard.SetText(r);
            }, _ => windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

            hotKeys.NewFile.OnEvent(async e =>
            {
                e.Handled = true;
                const string newFileName = "NewFile";
                var          handle      = windowManager.CurrentWindow.Handle;
                var          fullPath    = await fileExplorer.CurrentDirectory(handle);
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
                e.BeginInvoke(()=>keyboard.Type(Key.F2));
            }, _ => windowManager.CurrentWindow.IsExplorer);

            hotKeys.ShowDesktopFolder.OnEvent(e =>
            {
                fileExplorer.Open(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                e.Handled = true;
            });

            (Ctrl + Back).OnDown(e =>
            {
                if (windowManager.CurrentWindow.IsExplorer) // fix ctrl+back is a box char in explorer
                {
                    e.DisableVirtualKeyHandlingInThisEvent();
                    e.Handled = true;
                    keyboard.Type(Ctrl+Shift+Left, Back); // Ctrl is up now
                    keyboard.Down(Ctrl); //to trigger, if user hold ctrl and press back again
                }
            });
        }
    }
}