using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;

namespace Metatool.Service;

public interface IUIElement
{
    int X { get; }
    int Y { get; }
    int Width { get; }
    int Height { get; }
}
public interface IScreenHint
{
	/// <summary>
	/// show operational objects hints of the current active window, and do action when use select one by type the hint keys.
	/// </summary>
	Task Show(Action<(IUIElement winRect, IUIElement clientRect)> action, bool buildHints = true, bool activeWindowOnly = false, bool useWpfDetector = false);
	string HintKeys { get; set;}
}