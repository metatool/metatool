using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Metatool.Utils.Implementation;
using Metatool.Utils.Internal;
using Shell32;

namespace Metatool.Service;

public class FileExplorer : IFileExplorer
{
	public  async Task<string[]> GetSelectedPaths(IntPtr hWnd)
	{
		return await UiDispatcher.DispatchAsync<string[]>(() =>
			{
				var selected = new List<string>();
				var shellWindows = new SHDocVw.ShellWindows();
				foreach (SHDocVw.InternetExplorer window in shellWindows)
				{
					if (window.HWND != (int) hWnd) continue;

					if (!(window.Document is IShellFolderViewDual2 shellWindow)) continue; // not explorer

					var items = shellWindow.SelectedItems();
					foreach (Shell32.FolderItem item in items)
					{
						selected.Add(item.Path);
					}

					break;
				}

				return selected.ToArray();
			}
		);
	}

	public  async Task<string> CurrentDirectory(IntPtr hWnd)
	{
		return await UiDispatcher.DispatchAsync<string>(() =>
		{
			var shellWindows = new SHDocVw.ShellWindows();
			foreach (SHDocVw.InternetExplorer window in shellWindows)
			{
				if (window.HWND != (int) hWnd) continue;

				if (!(window.Document is IShellFolderViewDual2 shellWindow)) continue;

				// Item without an index returns the current object
				var currentFolder = shellWindow.Folder.Items().Item();

				// special folder - use window title
				// for some reason on "Desktop" gives null
				if (currentFolder == null || currentFolder.Path.StartsWith("::"))
				{
					// Get window title instead
					return WindowHelper.GetText(hWnd);
				}
				else
				{
					return currentFolder.Path;
				}
			}

			return string.Empty;
		});
	}

	public async Task Select(string[] fileNames, IntPtr? hWnd = null)
	{
		await UiDispatcher.DispatchAsync<bool>(() =>
		{
			var result = FindShellWindow(hWnd);
			if (result is not { } r) return false;
			var (window, shellWindow) = r;

			var selected = shellWindow.SelectedItems();
			for (var i = 0; i < selected.Count; i++)
			{
				shellWindow.SelectItem(selected.Item(i), 0); // unselect selected
			}

			// Refresh so the user sees newly created files in the view
			window.Refresh();

			// ParseName resolves against the filesystem, not the view cache,
			// so it finds new files immediately without waiting for refresh
			var folder = shellWindow.Folder;
			foreach (var fileName in fileNames)
			{
				var name = Path.GetFileName(fileName);
				var file = folder.ParseName(name);
				if (file != null)
					shellWindow.SelectItem(file, 1);
			}
			return true;
		});
	}

	// When hWnd is null, uses the foreground window as the target
	private static (SHDocVw.InternetExplorer, IShellFolderViewDual2)? FindShellWindow(IntPtr? hWnd)
	{
		var target = hWnd ?? PInvokes.GetForegroundWindow();
		var shellWindows = new SHDocVw.ShellWindows();
		foreach (SHDocVw.InternetExplorer window in shellWindows)
		{
			if (window.HWND != (int) target) continue;
			if (window.Document is IShellFolderViewDual2 shellWindow)
				return (window, shellWindow);
		}
		return null;
	}

	public  string Open(string path)
	{
		if (!Directory.Exists(path)) return "path not exists";
		Process.Start("explorer.exe", path);
		return "";
	}

}