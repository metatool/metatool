using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Metatool.Service;

public interface IVirtualDesktopManager
{
	Task<Process> GetFirstProcessOnCurrentVirtualDesktop(string exeName,
		Func<Process, bool> predict = null);

	Task<IEnumerable<Process>> GetProcessesOnCurrentVirtualDesktop(string exeName, Func<Process, bool> predict=null);
	Task<bool> IsOnCurrentVirtualDesktop(IntPtr hWnd);
	Task<Guid> GetWindowDesktopId(IntPtr hWnd);
	Task MoveWindowToDesktop(IntPtr hWnd, Guid currentDesktop);
}