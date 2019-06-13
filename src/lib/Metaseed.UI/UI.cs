using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using Metaseed.UI.Implementation;

namespace Metaseed.UI
{
    public class UI
    {
        static Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        public static void Dispatch(Action action)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Send, action);
        }
        public static string CurrentWindowClass
        {
            get
            {
                var hWnd = PInvokes.GetForegroundWindow();
                return WindowManager.GetClassName(hWnd);
            }
        }

        public static IntPtr CurrentWindowHandle => PInvokes.GetForegroundWindow();


        public static void FocusControl(string className, string text)
        {
            var hWnd = PInvokes.GetForegroundWindow();
            var hControl = PInvokes.FindWindowEx(hWnd, IntPtr.Zero, className, text);
            PInvokes.SetFocus(hControl);
        }

    }
}
