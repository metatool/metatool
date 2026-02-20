using System;
using System.Threading.Tasks;

namespace Metatool.Service;

public interface IFileExplorer
{
	Task<string[]> GetSelectedPaths(IntPtr hWnd);
	Task<string> CurrentDirectory(IntPtr hWnd);
	/// <summary>
	/// hWnd: if null, uses the foreground(active) window as the target
	/// </summary>
	Task Select(string[] fileNames, IntPtr? hWnd = null);
	string Open(string path);
}