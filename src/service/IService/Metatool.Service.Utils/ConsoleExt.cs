using System;
using System.Runtime.InteropServices;
using Metatool.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Metatool.Service
{
    public static class ConsoleExt
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

        // https://msdn.microsoft.com/fr-fr/library/windows/desktop/ms683242.aspx
        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);
        static SetConsoleCtrlEventHandler handlerDelegate = new SetConsoleCtrlEventHandler(Handler);
        private static bool Handler(CtrlType signal)
        {
            switch (signal)
            {
                case CtrlType.CTRL_BREAK_EVENT:
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    Exit?.Invoke();
                    Services.Get<ILogger<Object>>()?.LogInformation("exit: Ctrl Handler");
                    Context.Exit(0);
                    return false;

                default:
                    return false;
            }
        }
        private enum CtrlType
        {
            CTRL_C_EVENT        = 0,
            CTRL_BREAK_EVENT    = 1,
            CTRL_CLOSE_EVENT    = 2,
            CTRL_LOGOFF_EVENT   = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        private const int MF_BYCOMMAND = 0x00000000;
        public const  int SC_CLOSE     = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();
        public static event Action Exit;
        public enum SW : int
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,

            /// <summary>
            /// Activates and displays a window. If the window is minimized or
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window
            /// for the first time.
            /// </summary>
            Normal = 1,

            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,

            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?

            /// <summary>
            /// Activates the window and displays it as a maximized window.
            /// </summary>
            ShowMaximized = 3,

            /// <summary>
            /// Displays a window in its most recent size and position. This value
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,

            /// <summary>
            /// Activates the window and displays it in its current size and position.
            /// </summary>
            Show = 5,

            /// <summary>
            /// Minimizes the specified window and activates the next top-level
            /// window in the Z order.
            /// </summary>
            Minimize = 6,

            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,

            /// <summary>
            /// Displays the window in its current size and position. This value is
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
            /// window is not activated.
            /// </summary>
            ShowNA = 8,

            /// <summary>
            /// Activates and displays the window. If the window is minimized or
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,

            /// <summary>
            /// Sets the show state based on the SW_* value specified in the
            /// STARTUPINFO structure passed to the CreateProcess function by the
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,

            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
            /// that owns the window is not responding. This flag should only be
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, SW nCmdShow);
        public static void InitialConsole(bool disableCloseButton = false)
        {
            AllocConsole();

            var handle = GetConsoleWindow();
#if !DEBUG
            PInvokes.ShowWindowAsync(handle, PInvokes.SW.Hide);
            if(disableCloseButton)  DisableCloseButton();
#endif
            SetConsoleCtrlHandler(handlerDelegate, true);
            Console.CancelKeyPress += (_, __) =>
            {
                var config = Services.Get<IConfiguration>();
                var exit = config.GetValue<bool>("CtrlCExit");
                if (!exit)
                {
                    Services.CommonLogger.LogInformation("Ctrl+C exit disabled, to exit with Ctrl+C,please config CtrlCExit = true ");
                    return;
                }
                Exit?.Invoke();
                Services.CommonLogger.LogInformation("exist: Ctrl+C");
                var notify = Services.Get<INotify>();
                notify.ShowMessage("MetaKeyBoard Closing...");
                Context.Exit(0);
            };

        }

        public static void DisableCloseButton() =>
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

        public static void ShowConsole()
        {
            var handle = GetConsoleWindow();
            if (handle == IntPtr.Zero)
            {
                AllocConsole();
                return;
            }

            ShowWindowAsync(handle, SW.Show);
        }

        public static void HideConsole()
        {
            var handle = GetConsoleWindow();
            ShowWindowAsync(handle, SW.Hide);
        }
    }
}
