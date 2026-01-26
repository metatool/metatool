using System;
using System.Threading.Tasks;
using System.Windows;

namespace Metatool.Service;

public interface IScreenHint
{
	/// <summary>
	/// show operational objects hints of the current active window, and do action when use select one by type the hint keys.
	/// </summary>
	Task Show(Action<(Rect winRect, Rect clientRect)> action, bool buildHints = true);
}