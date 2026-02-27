using Metatool.Service;
using Metatool.Service.MouseKey;
using System;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using static Metatool.Service.MouseKey.Key;

namespace Metatool.Tools.WinShell;

public class FileExplorer : CommandPackage
{

	public FileExplorer(IWindowManager windowManager, IFileExplorer fileExplorer, IClipboard clipboard,IUiDispatcher dispatcher, IConfig<Config> config, INotify notify, IKeyboard keyboard)
	{
		RegisterCommands();
		var hotKeys = config.CurrentValue.FileExplorerPackage.Hotkeys;
		hotKeys.FocusItemsView.OnEvent(e =>
		{
			var listBoxEle = windowManager.CurrentWindow?.FirstDescendant(c => c.ByClassName("UIItemsView"));
			var selectedItem = listBoxEle?.SelectedItems()?.FirstOrDefault();
			if (selectedItem != null) selectedItem.SetFocus();
			else listBoxEle.FirstChild(c => c.ByClassName("UIItem"))?.Select();
			e.Handled = true;
		}, _ => windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

		hotKeys.FocusNavigationTreeView.OnEvent(e =>
		{
			var winEle = windowManager.CurrentWindow?.FirstDescendant(cf => cf.ByClassName("SysTreeView32"));
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
			var paths = await fileExplorer.GetSelectedPaths(handle);
			var r = string.Join(';', paths);
			notify.ShowMessage($"Path Copied: {r}");
            clipboard.SetText(r);

        }, _ => windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog);

		hotKeys.PasteAsFile.OnEvent(async e =>
		{
			e.Handled = true;
            var path = await clipboard.PasteAsFile();
            if (path == null) return;

            await fileExplorer.Select([path]);
            notify.ShowMessage($"file saved: {path}");
        }, _ => windowManager.CurrentWindow.IsExplorer);

        hotKeys.NewFile.OnEvent(async e =>
		{
			const string newFileName = "NewFile";
			var handle = windowManager.CurrentWindow.Handle;
			var fullPath = await fileExplorer.CurrentDirectory(handle);
			var fileName = newFileName;
			var i = 1;
			while (File.Exists(fullPath + "\\" + fileName))
			{
				fileName = newFileName + i++;
			}

			var file = File.Create(fullPath + "\\" + fileName);
			file.Close();
			await fileExplorer.Select([fileName], handle);
            keyboard.Type(Key.F2); // have to be triggered when no other keys are down.
        }, _ => windowManager.CurrentWindow.IsExplorer);

		hotKeys.ShowDesktopFolder.OnEvent(e =>
		{
			fileExplorer.Open(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
			e.Handled = true;
		});

		// When rename a file name: ab cd ef|
		// to delete ef, we press ctrl + backspace, but it will become "ab cd ef'boxChar'"
		// to fix it:
		(Ctrl + Back).OnDown(e =>
		{
			// fix ctrl+back is a box char in explorer
			e.DisableVirtualKeyHandlingInThisEvent();
			e.Handled = true;
			keyboard.Type(Ctrl + Shift + Left, Back); // Ctrl is up now
			keyboard.Down(Ctrl); //to trigger, if user hold ctrl and press back again
		},
			e => windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog,
			"fix ctrl+backspace function to delete a word in rename");
	}
}
