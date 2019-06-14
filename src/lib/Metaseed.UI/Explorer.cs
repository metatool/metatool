using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Metaseed.UI.Implementation;
using Shell32;

namespace Metaseed.UI
{
    public class Explorer
    {
        public static string[] GetSelectedFilePath(IntPtr hWnd)
        {
            var selected = new List<string>();
            var shellWindows = new SHDocVw.ShellWindows();
            foreach (SHDocVw.InternetExplorer window in shellWindows)
            {
                if (window.HWND != (int)hWnd) continue;

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

        public static string Path(IntPtr hWnd)
        {
            var shellWindows = new SHDocVw.ShellWindows();
            foreach (SHDocVw.InternetExplorer window in shellWindows)
            {
                if (window.HWND != (int)hWnd) continue;

                if (!(window.Document is IShellFolderViewDual2 shellWindow)) continue;

                // Item without an index returns the current object
                var currentFolder = shellWindow.Folder.Items().Item();

                // special folder - use window title
                // for some reason on "Desktop" gives null
                if (currentFolder == null || currentFolder.Path.StartsWith("::"))
                {
                    // Get window title instead
                    return WindowManager.GetText(hWnd);
                }
                else
                {
                    return currentFolder.Path;
                }
            }

            return string.Empty;
        }

        public static void Select(IntPtr hWnd, string[] fileNames)
        {
            var shellWindows = new SHDocVw.ShellWindows();
            foreach (SHDocVw.InternetExplorer window in shellWindows)
            {
                if (window.HWND != (int)hWnd) continue;

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
    }
}
