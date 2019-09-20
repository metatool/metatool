using System;
using System.Runtime.InteropServices;
using Metatool.UI.Implementation;

namespace Metatool.UI
{
    public static class ConsoleExt
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

        // https://msdn.microsoft.com/fr-fr/library/windows/desktop/ms683242.aspx
        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);
        private static bool Handler(CtrlType signal)
        {
            switch (signal)
            {
                case CtrlType.CTRL_BREAK_EVENT:
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    System.Console.WriteLine("Closing");
                    Exit?.Invoke();
                    Environment.Exit(0);
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

        public static event Action Exit;

        public static void InitialConsole()
        {
            PInvokes.AllocConsole();
            var handle = PInvokes.GetConsoleWindow();
#if !DEBUG
            PInvokes.ShowWindowAsync(handle, PInvokes.SW.Hide);
#endif
            SetConsoleCtrlHandler(Handler, true);

        }

        public static void ShowConsole()
        {
            var handle = PInvokes.GetConsoleWindow();
            if (handle == IntPtr.Zero)
            {
                PInvokes.AllocConsole();
                return;
            }

            PInvokes.ShowWindowAsync(handle, PInvokes.SW.Show);
        }

        public static void HideConsole()
        {
            var handle = PInvokes.GetConsoleWindow();
            PInvokes.ShowWindowAsync(handle, PInvokes.SW.Hide);
        }
    }
}