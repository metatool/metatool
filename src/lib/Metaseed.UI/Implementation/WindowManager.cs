using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Metaseed.UI.Implementation
{
    public class WindowManager
    {
        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            static bool EnumWindow(IntPtr handle, IntPtr pointer)
            {
                var gch = GCHandle.FromIntPtr(pointer);
                var list = gch.Target as List<IntPtr>;
                if (list == null)
                {
                    throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
                }
                list.Add(handle);
                //  You can modify this to check to see if you want to cancel the operation, then return a null here
                return true;
            }
            var result = new List<IntPtr>();
            var listHandle = GCHandle.Alloc(result);
            try
            {
                var childProc = new EnumWindowsProc(EnumWindow);
                PInvokes.EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        public static List<IntPtr> GetAllWindows()
        {

            static bool GetWindowHandle(IntPtr windowHandle, ArrayList windowHandles)
            {
                windowHandles.Add(windowHandle);
                return true;
            }
            var windowHandles = new ArrayList(); 
            EnumWindowsProc_List callBackPtr = GetWindowHandle;
            PInvokes.EnumWindows(callBackPtr, windowHandles);

            foreach (IntPtr windowHandle in windowHandles.ToArray())
            {
                PInvokes.EnumChildWindows(windowHandle, callBackPtr, windowHandles);
            }

            return windowHandles.Cast<IntPtr>().ToList();
        }

        // Find window by Caption, and wait 1/2 a second and then try again.
        public static IntPtr FindWindow(string className, string windowName, bool wait = false, int retryIntervalMs = 500, int retryTimes = 5)
        {
            var hWnd = PInvokes.FindWindow(className, windowName);
            while (wait && retryTimes-- > 0 && hWnd == IntPtr.Zero)
            {
                System.Threading.Thread.Sleep(retryIntervalMs);
                hWnd = PInvokes.FindWindow(null, windowName);
            }

            return hWnd;
        }

    
        public static bool BringWindowToTop(string className, string windowName, bool wait)
        {
            var hWnd = FindWindow(className, windowName, wait);
            if (hWnd != IntPtr.Zero)
            {
                return PInvokes.SetForegroundWindow(hWnd);
            }
            return false;
        }

        public static string GetText(IntPtr hWnd)
        {
            // Allocate correct string length first
            var length = PInvokes.GetWindowTextLength(hWnd);
            var sb = new StringBuilder(length + 1);
            PInvokes.GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string GetWindowTextRaw(IntPtr hwnd)
        {
            // Allocate correct string length first
            var length = (int)PInvokes.SendMessage(hwnd,(int) WM.GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
            var sb = new StringBuilder(length + 1);
            PInvokes.SendMessage(hwnd, (int)WM.GETTEXT, (IntPtr)sb.Capacity, sb);
            return sb.ToString();
        }

        public static string GetClassName(IntPtr hWnd)
        {
            StringBuilder className = new StringBuilder(512);
            var r =  PInvokes.GetClassName(hWnd, className, className.Capacity);
            if (r != 0)
            {
                return className.ToString();
            }

            return string.Empty;
        }
    }
}
