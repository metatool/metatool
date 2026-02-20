using System.Collections.Generic;
using System.Windows;

namespace Metatool.ScreenPoint;

public interface IHintsBuilder
{
	/// <summary>
	/// Assigns keyboard shortcut keys to each element's bounding rectangle
	/// from the pre-detected UI elements.
	/// </summary>
	/// <param name="elementRects">The list of element bounding rectangles relative to the window.</param>
	/// <returns>
	/// A tuple containing the window's bounding rectangle and a dictionary mapping
	/// key sequences to the relative bounding rectangles of each UI element.
	/// </returns>
	Dictionary<string, Rect> BuildHintPositions(List<Rect> elementRects);
}
