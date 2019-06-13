using System;
using System.Collections.Generic;
using System.Text;
using Metaseed.UI.Implementation;

namespace Metaseed.UI
{
    public class UI
    {
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
