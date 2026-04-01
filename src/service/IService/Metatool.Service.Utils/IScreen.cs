using System;

namespace Metatool.Service;

public interface IScreen
{
	/// <summary>
	/// Gets all screens organized as a 2D array, where screens are grouped by row based on height center proximity.
	/// </summary>
	/// <returns>A 2D array of screen rectangles, where each row may have different length</returns>
	System.Drawing.Rectangle[][] IdentifyScreens();

	/// <summary>
	/// Activates (brings to foreground) the topmost window on the specified screen.
	/// </summary>
	/// <param name="rowIndex">The row index of the screen</param>
	/// <param name="columnIndex">The column index of the screen</param>
	void ActivateTopWindowOnScreen(int rowIndex, int columnIndex);
}
