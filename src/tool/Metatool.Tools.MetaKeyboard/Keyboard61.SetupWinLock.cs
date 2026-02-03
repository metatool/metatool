using System;
using System.Windows;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Metatool.MetaKeyboard
{
	partial class Keyboard61
	{
		/// <summary>
		/// goal: liberate the *+Win+L to be used by other commands, i.e. moving window to another screen with shift+win+space+l,
		/// by default it will trigger the system win lock, i.e. win+z+l will lock too.
		///	so we want to but only enable the winLock with exact LWin+L hotkey.
		///
		/// solution: disable the Win+L by config the registry key at startup, when the exact LWin+L hotkey down enable WinLock,
		///		then the next down of L(the second repeating down when long pressing) will trigger the WinLock, when the L is up, disable the winLock again.
		/// problem: when the keyboard of the app is in a wrong state. i.e. A is down, we can not lock the screen.
		/// solution: create a keyboard state reset command() to clear all down keys. but need to remember the hot key to use it.
		/// idea(todo): can we period sync system keyboard state to the app keyboard state or auto reset all key down? i.e. 3 seconds when no key is pressed.
		/// </summary>
		private void SetupWinLock()
		{
			void EnableWinLock()
			{
				try
				{
					Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System")
					?.SetValue("DisableLockWorkstation", 0, RegistryValueKind.DWord);
					Logger.LogInformation("WinLock Enabled");
				}
				catch (UnauthorizedAccessException)
				{
					Logger.LogWarning(
						"Could not enable WinLock(Win+L), so *+Win+L would trigger ScreenLock, press LCtrl+LWin+LAlt+X to restart with Admin rights.");
				}
			}

			void DisableWinLock()
			{
				try
				{
					Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System")
					?.SetValue("DisableLockWorkstation", 1, RegistryValueKind.DWord);
					Logger.LogInformation("WinLock disabled!");
				}
				catch (UnauthorizedAccessException)
				{
					Logger.LogWarning(
						"Could not disable WinLock(Win+L), so *+Win+L would trigger ScreenLock, press LCtrl+LWin+LAlt+X to restart with Admin rights.");
				}
			}

			DisableWinLock();
			var winLock = Key.Win + Key.L;
			var winOrLKey = Key.Win | Key.L | Key.Ctrl | Key.Alt | Key.Shift;
			var enableLock = winLock.OnDown(e =>
			{
				// only when LWin+L pressed, enable to lock
				EnableWinLock();
				// next the down event go to the system handler, it will trigger the win lock function
			},
				// *+win+L disable winlock
				e => !e.KeyboardState.IsOtherDown(winOrLKey), description: "Enable WinLock for Win+L");

			var disableLock = winLock.OnUp(e =>
			{
				// disable again, so when unlocked,
				DisableWinLock();
			},
				// *+Win+L up would not trigger this handler
				e => !e.KeyboardState.IsOtherDown(winOrLKey), description: "Disable WinLock for Win+L");

			// when the plugin application exits, reenable normal handling
			Application.Current.Dispatcher.BeginInvoke((Action)(() =>
			{
				Application.Current.DispatcherUnhandledException += (_, __) => EnableWinLock();
				AppDomain.CurrentDomain.UnhandledException += (_, __) => EnableWinLock();
				//Application.Current.Exit                         += (_, __) => EnableWinLock();
				Console.CancelKeyPress += (_, __) => EnableWinLock();
				ConsoleExt.Exit += EnableWinLock;
			}));


		}
	}
}