using System.Windows.Forms;

namespace Metatool.Service;

public interface IScreen
{
	/// <summary>
	/// Gets all screens organized as a 2D array, grouped by row based on height center proximity.
	/// [0][0] is the top-left screen, rows top-to-bottom, columns left-to-right.
	/// </summary>
	Screen[][] Screens { get; }

	/// <summary>
	/// Activates (brings to foreground) the topmost window on the specified screen.
	/// </summary>
	/// <param name="rowIndex">The row index of the screen</param>
	/// <param name="columnIndex">The column index of the screen</param>
	void ActivateTopWindowOnScreen(int rowIndex, int columnIndex);
}
