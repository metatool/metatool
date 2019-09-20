using System;
using System.Windows;
using Accessibility;
using Metatool.Input;
using Microsoft.Win32;

namespace ConsoleApp1
{
    partial class Keyboard61
    {
        private static void SetupWinLock()
        {
            static void EnableWinLock()
            {
                Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System")?
                    .SetValue("DisableLockWorkstation", 0, RegistryValueKind.DWord);
            }

            static void DisableWinLock()
            {
                Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System")?
                    .SetValue("DisableLockWorkstation", 1, RegistryValueKind.DWord);
            }

            try
            {
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
                Application.Current.DispatcherUnhandledException += (_,__) => EnableWinLock();
                AppDomain.CurrentDomain.UnhandledException += (_, __) => EnableWinLock();
                Application.Current.Exit += (_, __) => EnableWinLock();
                Console.WriteLine("WinLock disabled!");
                
            }
            catch (Exception)
            {
                Console.WriteLine("Could not disable WinLock(Win+L), so *+Win+L would trigger ScreenLock, press LCtrl+LWin+LAlt+X to restart with Admin rights.");
            }

        }
    }
}
