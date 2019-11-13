using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using Metatool.Utils.Implementation;
using Metatool.Utils.Internal;
using Condition = System.Windows.Automation.Condition;
using Point = System.Drawing.Point;

namespace Metatool.Service
{
    public class Window : IWindow
    {
        public Window(IntPtr handle)
        {
            Handle = handle;
        }

        public IntPtr Handle { get; }
        public string Class  => WindowHelper.GetClassName(Handle);

        public Rect CaretPosition
        {
            get
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
        }

        public Rect Rect
        {
            get
            {
                PInvokes.GetWindowRect(Handle, out var rect);
                return new Rect(new System.Windows.Point() {X = rect.Left, Y  = rect.Top},
                    new System.Windows.Point() {X             = rect.Right, Y = rect.Bottom});
            }
        }

        public bool IsExplorerOrOpenSaveDialog
        {
            get
            {
                var c = Class;
                return "CabinetWClass" == c || "#32770" == c;
            }
        }

        public bool IsExplorer => "#CabinetWClass" == Class;

        public bool IsOpenSaveDialog => "#32770" == Class;

        public AutomationElement UiAuto => AutomationElement.FromHandle(Handle);

        public void FocusControl(string className, string text)
        {
            var hWnd     = PInvokes.GetForegroundWindow();
            var hControl = PInvokes.FindWindowEx(hWnd, IntPtr.Zero, className, text);
            PInvokes.SetFocus(hControl);
        }

        public AutomationElement FirstChild(Func<ConditionFactory, Condition> condition) => UiAuto.First( TreeScope.Children, condition);

        public AutomationElement FirstDescendant(Func<ConditionFactory, Condition> condition) => UiAuto.First( TreeScope.Descendants, condition);
    }
}