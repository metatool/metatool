using System;
using System.Collections.Generic;
using System.Windows;

namespace Metatool.ScreenPoint;

public interface IUIElementsDetector
{
	/// <summary>
	/// Detects all visible, enabled UI automation elements within the specified window
	/// and returns their bounding rectangles relative to the window's top-left corner.
	/// </summary>
	/// <param name="winHandle">The native window handle to inspect.</param>
	/// <returns>
	/// A tuple containing the window's absolute bounding rectangle and a list of
	/// element bounding rectangles with coordinates relative to the window.
	/// </returns>
	(Rect winRect, List<Rect> rects) Detect(IntPtr winHandle);
}
