using System;
using System.Windows;
using Accessibility;
using Metatool.Input;
using Metatool.UI;
using Microsoft.Win32;

namespace ConsoleApp1
{
    partial class Keyboard61
    {
        private static void SetupWinLock()
        {
            static void EnableWinLock()
            {
                try
                {
                    Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System")?
                        .SetValue("DisableLockWorkstation", 0, RegistryValueKind.DWord);
                }
                catch (Exception)
                {
                    Console.WriteLine(
                        "Could not enable WinLock(Win+L), so *+Win+L would trigger ScreenLock, press LCtrl+LWin+LAlt+X to restart with Admin rights.");
                }
            }

            static void DisableWinLock()
            {
                try
                {
                    Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System")?
                        .SetValue("DisableLockWorkstation", 1, RegistryValueKind.DWord);
                }
                catch (Exception)
                {
                    Console.WriteLine(
                        "Could not disable WinLock(Win+L), so *+Win+L would trigger ScreenLock, press LCtrl+LWin+LAlt+X to restart with Admin rights.");
                }
            }

            DisableWinLock();
            var winLock   = Key.Win + Key.L;
            var winOrLKey = Key.Win | Key.L;
            var enableLock = winLock.Down(e =>
            {
                if (e.KeyboardState.IsOtherDown(winOrLKey)) return;
                // when LWin+L pressed, enable to lock
                EnableWinLock();
            });
            var disableLock = winLock.Up(e =>
            {
                if (e.KeyboardState.IsOtherDown(winOrLKey)) return;

                // disable again, so when unlocked, *+Win+L would not trigger screen locking
                DisableWinLock();
            });
            Application.Current.DispatcherUnhandledException += (_, __) => EnableWinLock();
            AppDomain.CurrentDomain.UnhandledException       += (_, __) => EnableWinLock();
            Application.Current.Exit                         += (_, __) => EnableWinLock();
            Console.CancelKeyPress                           += (_, __) => EnableWinLock();
            ConsoleExt.Exit                                  += EnableWinLock;

            Console.WriteLine("WinLock disabled!");
        }
    }
}