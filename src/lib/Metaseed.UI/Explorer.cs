using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.UI
{
    public class Explorer
    {
        public static string[] GetSelectedFilePath(IntPtr handle)
        {

            List<string> selected = new List<string>();
            var shellWindows = new SHDocVw.ShellWindows();
            foreach (SHDocVw.InternetExplorer window in shellWindows)
            {
                if (window.HWND == (int)handle)
                {
                    Shell32.FolderItems items = ((Shell32.IShellFolderViewDual2)window.Document).SelectedItems();
                    foreach (Shell32.FolderItem item in items)
                    {
                        selected.Add(item.Path);
                    }
                }
            }
            return selected.ToArray();
        }

    }
}
