using System;
using System.Windows;
using Metatool.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Metatool.MetaKeyboard
{
	partial class Keyboard61
	{
		private void SetupWinLock()
		{
			void EnableWinLock()
			{
				try
				{
					Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System")?
						.SetValue("DisableLockWorkstation", 0, RegistryValueKind.DWord);
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
					Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System")?
						.SetValue("DisableLockWorkstation", 1, RegistryValueKind.DWord);
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
				e => !e.KeyboardState.IsOtherDown(winOrLKey));

			var disableLock = winLock.OnUp(e =>
			{
				// disable again, so when unlocked,
				DisableWinLock();
			},
				// *+Win+L up would not trigger this handler
				e => !e.KeyboardState.IsOtherDown(winOrLKey));

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