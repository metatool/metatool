using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Metatool.Utils.Implementation;
using Shell32;

namespace Metatool.Service
{
    public class FileExplorer : IFileExplorer
    {
        public  async Task<string[]> GetSelectedPath(IntPtr hWnd)
        {
            return await UiDispacher.DispatchAsync<string[]>(() =>
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

        public  async Task<string> Path(IntPtr hWnd)
        {
            return await UiDispacher.DispatchAsync<string>(() =>
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

        public  void Select(IntPtr hWnd, string[] fileNames)
        {
            var shellWindows = new SHDocVw.ShellWindows();
            foreach (SHDocVw.InternetExplorer window in shellWindows)
            {
                if (window.HWND != (int) hWnd) continue;

                if (!(window.Document is IShellFolderViewDual2 shellWindow)) continue;

                var selected = shellWindow.SelectedItems();

                for (var i = 0; i < selected.Count; i++)
                {
                    shellWindow.SelectItem(selected.Item(i), 0); // unselect selected
                }

                window.Refresh();

                foreach (var fileName in fileNames)
                {
                    var file = selected.Item(fileName);
                    shellWindow.SelectItem(file, 1);
                }
            }
        }

        public  string Open(string path)
        {
            if (!Directory.Exists(path)) return "path not exists";
            Process.Start("explorer.exe", path);
            return "";
        }

    }
}
