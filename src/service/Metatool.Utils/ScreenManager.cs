using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Metatool.Utils.Internal;
using Microsoft.Extensions.Logging;
using Rectangle = System.Drawing.Rectangle;

namespace Metatool.Service;

public class ScreenManager : IScreen
{
	private readonly ILogger _logger;
	private readonly Rectangle[][] _cachedScreens;

	public ScreenManager(ILogger<ScreenManager> logger)
	{
		_logger = logger;
		_cachedScreens = IdentifyScreens();
	}

	/// <summary>
	/// Enumerates all screens and organizes them as a 2D array grouped by row.
	/// [0][0] is the top-left screen, rows top to bottom, columns left to right.
	/// </summary>
	public Rectangle[][] IdentifyScreens()
	{
		var screens = System.Windows.Forms.Screen.AllScreens;
		if (screens.Length == 0)
			return [];

		// Group screens by row based on height center proximity
		var screenList = screens.Select(s => s.Bounds).ToList();
		var rows = GroupScreensByRow(screenList);

		// Sort each row by X position (left to right)
		foreach (var row in rows)
		{
			row.Sort((a, b) => a.X.CompareTo(b.X));
		}

		return [..rows.Select(r => r.ToArray())];
	}

	public void ActivateTopWindowOnScreen(int rowIndex, int columnIndex)
	{
		var screens = _cachedScreens;

		// Validate indices
		if (rowIndex < 0 || rowIndex >= screens.Length)
		{
			_logger.LogWarning("ActivateTopWindowOnScreen: invalid rowIndex {Row}, screen rows: {Rows}", rowIndex, screens.Length);
			return;
		}

		if (columnIndex < 0 || columnIndex >= screens[rowIndex].Length)
		{
			_logger.LogWarning("ActivateTopWindowOnScreen: invalid columnIndex {Col}, row {Row} has {Cols} columns", columnIndex, rowIndex, screens[rowIndex].Length);
			return;
		}

		var targetScreen = screens[rowIndex][columnIndex];

		// Get the HMONITOR handle for the target screen via its bounds
		var monitorRect = new PInvokes.RECT
		{
			Left = targetScreen.Left, Top = targetScreen.Top,
			Right = targetScreen.Right, Bottom = targetScreen.Bottom
		};
		var targetMonitor = PInvokes.MonitorFromRect(ref monitorRect, PInvokes.MONITOR_DEFAULTTONEAREST);

		// Find all windows in Z-order (EnumWindows returns top to bottom)
		var windows = new List<IntPtr>();
		PInvokes.EnumWindows((hwnd, _) =>
		{
			windows.Add(hwnd);
			return true;
		}, IntPtr.Zero);

		var shellWindow = PInvokes.GetDesktopWindow();
		_logger.LogDebug("ActivateTopWindowOnScreen[{Row},{Col}]: target={Screen}, monitor=0x{Monitor:X}, windows={Count}",
			rowIndex, columnIndex, targetScreen, targetMonitor.ToInt64(), windows.Count);

		// Find the topmost Alt+Tab-style window on the target screen
		foreach (var hwnd in windows)
		{
			if (hwnd == shellWindow)
				continue;

			if (!PInvokes.IsWindowVisible(hwnd))
				continue;

			// Skip cloaked windows (hidden UWP apps, virtual desktop windows, etc.)
			PInvokes.DwmGetWindowAttribute(hwnd, PInvokes.DWMWA_CLOAKED, out var cloaked, sizeof(int));
			if (cloaked != 0)
				continue;

			// Filter by window style: skip tool windows and noactivate windows,
			// unless they explicitly have WS_EX_APPWINDOW
			var exStyle = PInvokes.GetWindowLong(hwnd, PInvokes.GWL_EXSTYLE);
			if ((exStyle & PInvokes.WS_EX_APPWINDOW) == 0)
			{
				if ((exStyle & PInvokes.WS_EX_TOOLWINDOW) != 0)
					continue;
				if ((exStyle & PInvokes.WS_EX_NOACTIVATE) != 0)
					continue;
				// Skip owned windows without WS_EX_APPWINDOW (they don't appear in Alt+Tab)
				if (PInvokes.GetWindow(hwnd, PInvokes.GetWindowType.GW_OWNER) != IntPtr.Zero)
					continue;
			}

			// Use MonitorFromWindow to determine which monitor this window belongs to.
			// Handles windows spanning monitors (uses largest area) and minimized windows correctly.
			var windowMonitor = PInvokes.MonitorFromWindow(hwnd, PInvokes.MONITOR_DEFAULTTONULL);
			if (windowMonitor == IntPtr.Zero || windowMonitor != targetMonitor)
				continue;

			var title = GetWindowTitle(hwnd);
			var className = GetWindowClassName(hwnd);
			_logger.LogDebug("  candidate: hwnd=0x{Hwnd:X}, title=\"{Title}\", class=\"{Class}\", monitor=0x{Monitor:X}",
				hwnd.ToInt64(), title, className, windowMonitor.ToInt64());

			var minimized = PInvokes.IsIconic(hwnd);
			PInvokes.SetForegroundWindow(hwnd);
			if (minimized)
				PInvokes.ShowWindowAsync(hwnd, PInvokes.SW.Restore);
			new Window(hwnd).Highlight();
			return;
		}

		_logger.LogWarning("ActivateTopWindowOnScreen[{Row},{Col}]: no matching window found on screen {Screen}", rowIndex, columnIndex, targetScreen);
	}

	private static string GetWindowTitle(IntPtr hwnd)
	{
		var sb = new StringBuilder(256);
		PInvokes.GetWindowText(hwnd, sb, sb.Capacity);
		return sb.ToString();
	}

	private static string GetWindowClassName(IntPtr hwnd)
	{
		var sb = new StringBuilder(256);
		PInvokes.GetClassName(hwnd, sb, sb.Capacity);
		return sb.ToString();
	}

	/// <summary>
	/// Groups screens by row based on height center proximity.
	/// Screens are considered in the same row if their height centers differ by less than half the max height.
	/// </summary>
	private static List<List<Rectangle>> GroupScreensByRow(List<Rectangle> screens)
	{
		var rows = new List<List<Rectangle>>();

		if (screens.Count == 0)
			return rows;

		// Sort screens by Y position first
		var sortedByY = screens.OrderBy(s => s.Y).ToList();

		foreach (var screen in sortedByY)
		{
			var screenCenterY = screen.Y + screen.Height / 2;
			var found = false;

			// Try to find an existing row this screen belongs to
			foreach (var row in rows)
			{
				// Calculate the threshold based on the maximum height in the row
				var maxHeightInRow = row.Max(s => s.Height);
				var threshold = maxHeightInRow / 2;

				// Check if any screen in this row has a similar center Y
				foreach (var existingScreen in row)
				{
					var existingCenterY = existingScreen.Y + existingScreen.Height / 2;
					if (Math.Abs(screenCenterY - existingCenterY) < threshold)
					{
						row.Add(screen);
						found = true;
						break;
					}
				}

				if (found)
					break;
			}

			// If not found in any existing row, create a new row
			if (!found)
			{
				rows.Add(new List<Rectangle> { screen });
			}
		}

		return rows;
	}
}
