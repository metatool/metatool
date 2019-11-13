﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;
using Metatool.Utils.Implementation;
using Point = System.Drawing.Point;

namespace Metatool.Service
{
    public class Window
    {
        private static readonly Dispatcher Dispatcher = Application.Current.Dispatcher;

        internal static void Dispatch(Action action)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
        }

        internal static T Dispatch<T>(Func<T> action)
        {
            return (T)Dispatcher.Invoke(DispatcherPriority.Send, action);
        }

        internal static async Task<T> DispatchAsync<T>(Func<T> action)
        {
            var o = Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
            await o;
            return (T) (o.Result);
        }

        public static string CurrentWindowClass => WindowManager.GetClassName(PInvokes.GetForegroundWindow());

        public static IntPtr            CurrentWindowHandle => PInvokes.GetForegroundWindow();
        public static AutomationElement CurrentWindow       => AutomationElement.FromHandle(Service.Window.CurrentWindowHandle);

        public static void FocusControl(string className, string text)
        {
            var hWnd     = PInvokes.GetForegroundWindow();
            var hControl = PInvokes.FindWindowEx(hWnd, IntPtr.Zero, className, text);
            PInvokes.SetFocus(hControl);
        }

        public static void Show(IntPtr hWnd)
        {
            PInvokes.ShowWindowAsync(hWnd, PInvokes.SW.Show);
            PInvokes.SetForegroundWindow(hWnd);
        }

        public static Rect GetCurrentWindowCaretPosition()
        {
            var guiInfo = new GUITHREADINFO();
            guiInfo.cbSize = (uint) Marshal.SizeOf(guiInfo);

            PInvokes.GetGUIThreadInfo(0, out guiInfo);

            var lt = new Point((int) guiInfo.rcCaret.Left, (int) guiInfo.rcCaret.Top);
            var rb = new Point((int) guiInfo.rcCaret.Right, (int) guiInfo.rcCaret.Bottom);

            PInvokes.ClientToScreen(guiInfo.hwndCaret, out lt);
            PInvokes.ClientToScreen(guiInfo.hwndCaret, out rb);
            Console.WriteLine(lt.ToString() + rb.ToString());
            //SystemInformation.WorkingArea
            return new Rect(new System.Windows.Point() {X = lt.X, Y = lt.Y},
                new System.Windows.Point() {X             = rb.X, Y = rb.Y});
        }

        public static Rect GetCurrentWindowRect()
        {
            PInvokes.GetWindowRect(CurrentWindowHandle, out var rect);
            return new Rect(new System.Windows.Point() {X = rect.Left, Y  = rect.Top},
                new System.Windows.Point() {X             = rect.Right, Y = rect.Bottom});
        }

        public static bool IsExplorerOrOpenSaveDialog
        {
            get
            {
                var c = Service.Window.CurrentWindowClass;
                return "CabinetWClass" == c || "#32770" == c;
            }
        }

        public static bool IsExplorer => "#CabinetWClass" == Service.Window.CurrentWindowClass;
      
        public static bool IsOpenSaveDialog => "#32770" == Service.Window.CurrentWindowClass;
    }
}