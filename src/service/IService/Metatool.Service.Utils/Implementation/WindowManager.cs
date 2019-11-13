using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Metatool.Utils.Implementation
{
    internal class WindowManager
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

        public void FindWindows(IntPtr parentHandle, Regex className, Regex windowText, Regex process, Predicate<IntPtr> foundWindow)
        {
            bool EnumChildWindowsCallback(IntPtr handle, IntPtr lParam)
            {
                if (className != null)
                {
                    var sbClass = new StringBuilder(256);
                    PInvokes.GetClassName(handle, sbClass, sbClass.Capacity);

                    if (!className.IsMatch(sbClass.ToString()))
                        return true; // continue the enum
                }

                if (windowText != null)
                {
                    var           txtLength = PInvokes.SendMessage(handle,(int) WM.GETTEXTLENGTH, 0, IntPtr.Zero);
                    var sbText    = new StringBuilder(txtLength + 1);
                    PInvokes.SendMessage(handle, (int) WM.GETTEXT, (IntPtr) sbText.Capacity, sbText);

                    if (!windowText.IsMatch(sbText.ToString()))
                        return true; 
                }

                if (process != null)
                {
                    PInvokes.GetWindowThreadProcessId(handle, out var processID);

                    var p = Process.GetProcessById(processID);

                    if (!process.IsMatch(p.ProcessName))
                        return true;
                }

                return foundWindow(handle);
            }
            PInvokes.EnumChildWindows(parentHandle, new EnumWindowsProc(EnumChildWindowsCallback), IntPtr.Zero);
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
            var className = new StringBuilder(512);
            var r =  PInvokes.GetClassName(hWnd, className, className.Capacity);
            return r != 0 ? className.ToString() : "";
        }
    }
}
