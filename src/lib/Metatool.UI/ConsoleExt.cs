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
        private const int MF_BYCOMMAND = 0x00000000;
        public const  int SC_CLOSE     = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();


        public static event Action Exit;

        public static void InitialConsole(bool disableCloseButton = false)
        {
            PInvokes.AllocConsole();
            var handle = PInvokes.GetConsoleWindow();
#if !DEBUG
            PInvokes.ShowWindowAsync(handle, PInvokes.SW.Hide);
#endif
            SetConsoleCtrlHandler(Handler, true);
            if(disableCloseButton)  DisableCloseButton();
        }

        public static void DisableCloseButton() =>
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

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