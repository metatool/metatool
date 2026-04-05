using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Metatool.Utils.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Metatool.Service;

public class ScreenManager : IScreen
{
	private readonly ILogger _logger;
	private volatile Screen[][] _screens;

	public ScreenManager(ILogger<ScreenManager> logger)
	{
		_logger = logger;
		_screens = BuildScreenLayout();
		SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
	}

	/// <summary>
	/// All screens organized as a 2D array grouped by row.
	/// [0][0] is the top-left screen, rows top to bottom, columns left to right.
	/// Screens are considered in the same row if their height centers differ by less than half the max height in that row.
	/// </summary>
	public Screen[][] Screens => _screens;

	private void OnDisplaySettingsChanged(object sender, EventArgs e)
	{
		_logger.LogInformation("Display settings changed, refreshing screen layout");
		_screens = BuildScreenLayout();
	}

	public void ActivateTopWindowOnScreen(int rowIndex, int columnIndex)
	{
		var screens = _screens;

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

		if (_logger.IsEnabled(LogLevel.Debug))
			_logger.LogDebug("ActivateTopWindowOnScreen[{Row},{Col}]: target={Screen}",
				rowIndex, columnIndex, targetScreen.Bounds);

		var shellWindow = PInvokes.GetDesktopWindow();
		IntPtr found = IntPtr.Zero;

		// Find the topmost Alt+Tab-style window on the target screen.
		// EnumWindows walks top-to-bottom in Z-order — stop at the first match.
		PInvokes.EnumWindows((hwnd, _) =>
		{
			if (hwnd == shellWindow)
				return true;

			if (!IsAltTabWindow(hwnd))
				return true;

			// Use Screen.FromHandle to determine which monitor this window belongs to.
			// Handles windows spanning monitors (uses largest area) and minimized windows correctly.
			if (!Screen.FromHandle(hwnd).Equals(targetScreen))
				return true;

			found = hwnd;
			return false; // stop enumeration
		}, IntPtr.Zero);

		if (found == IntPtr.Zero)
		{
			_logger.LogWarning("ActivateTopWindowOnScreen[{Row},{Col}]: no matching window found on screen {Screen}", rowIndex, columnIndex, targetScreen.Bounds);
			return;
		}

		if (_logger.IsEnabled(LogLevel.Debug))
		{
			_logger.LogDebug("  activating: hwnd=0x{Hwnd:X}, title=\"{Title}\", class=\"{Class}\"",
				found.ToInt64(), GetWindowText(found), GetClassName(found));
		}

		var minimized = PInvokes.IsIconic(found);
		PInvokes.SetForegroundWindow(found);
		if (minimized)
			PInvokes.ShowWindowAsync(found, PInvokes.SW.Restore);
		new Window(found).Highlight();
	}

	/// <summary>
	/// Returns true if the window would appear in the Alt+Tab list.
	/// </summary>
	private static bool IsAltTabWindow(IntPtr hwnd)
	{
		if (!PInvokes.IsWindowVisible(hwnd))
			return false;

		// Skip cloaked windows (hidden UWP apps, virtual desktop windows, etc.)
		PInvokes.DwmGetWindowAttribute(hwnd, PInvokes.DWMWA_CLOAKED, out var cloaked, sizeof(int));
		if (cloaked != 0)
			return false;

		// Filter by window style: skip tool windows and noactivate windows,
		// unless they explicitly have WS_EX_APPWINDOW
		var exStyle = PInvokes.GetWindowLong(hwnd, PInvokes.GWL_EXSTYLE);
		if ((exStyle & PInvokes.WS_EX_APPWINDOW) != 0)
			return true;

		if ((exStyle & PInvokes.WS_EX_TOOLWINDOW) != 0)
			return false;
		if ((exStyle & PInvokes.WS_EX_NOACTIVATE) != 0)
			return false;

		// Skip owned windows without WS_EX_APPWINDOW (they don't appear in Alt+Tab)
		if (PInvokes.GetWindow(hwnd, PInvokes.GetWindowType.GW_OWNER) != IntPtr.Zero)
			return false;

		return true;
	}

	/// <summary>
	/// Enumerates all screens and organizes them as a 2D array grouped by row.
	/// </summary>
	private static Screen[][] BuildScreenLayout()
	{
		var allScreens = Screen.AllScreens;
		if (allScreens.Length == 0)
			return [];

		var rows = GroupScreensByRow(allScreens);
		return [..rows.Select(row => row.ToArray())];
	}

	/// <summary>
	/// Groups screens into rows by Y-center proximity, sorted left-to-right within each row.
	/// Screens are considered in the same row if their height centers differ by less than half the max height in that row.
	/// </summary>
	private static List<List<Screen>> GroupScreensByRow(Screen[] screens)
	{
		var rows = new List<List<Screen>>();

		// Sort screens by Y position first, then try to place each into an existing row
		foreach (var screen in screens.OrderBy(s => s.Bounds.Y))
		{
			var centerY = screen.Bounds.Y + screen.Bounds.Height / 2;

			// Try to find an existing row this screen belongs to:
			// threshold is half the max height in the row, so screens with similar
			// vertical centers (within that tolerance) are grouped together.
			var matchedRow = rows.FirstOrDefault(row =>
			{
				var threshold = row.Max(s => s.Bounds.Height) / 2;
				return row.Any(s => Math.Abs(centerY - (s.Bounds.Y + s.Bounds.Height / 2)) < threshold);
			});

			// If not found in any existing row, create a new row
			if (matchedRow != null)
				matchedRow.Add(screen);
			else
				rows.Add([screen]);
		}

		// Sort each row by X position (left to right)
		foreach (var row in rows)
			row.Sort((a, b) => a.Bounds.X.CompareTo(b.Bounds.X));

		return rows;
	}

	private static string GetWindowText(IntPtr hwnd)
	{
		var sb = new StringBuilder(256);
		PInvokes.GetWindowText(hwnd, sb, sb.Capacity);
		return sb.ToString();
	}

	private static string GetClassName(IntPtr hwnd)
	{
		var sb = new StringBuilder(256);
		PInvokes.GetClassName(hwnd, sb, sb.Capacity);
		return sb.ToString();
	}
}
