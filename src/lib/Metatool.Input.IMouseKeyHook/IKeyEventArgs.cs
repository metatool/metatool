using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Metatool.Input
{
    public interface IKeyEventArgs
    {
        bool Handled { get; set; }
        Keys KeyData { get; }
        Keys KeyCode { get; }
        int KeyValue { get; }
        bool Alt { get; }
        bool Control { get; }
        bool Shift { get; }
        bool NoFurtherProcess { get; set; }
        int           ScanCode         { get; }
        bool          IsVirtual        { get; }
        int           Timestamp        { get; }
        bool          IsKeyDown        { get; }
        KeyEvent KeyEvent { get; }
        IKeyboardState KeyboardState { get; }
        IKeyPath PathToGo { get; }
        bool IsKeyUp { get; }
        bool IsExtendedKey { get; }
        IKeyEventArgs LastKeyDownEvent { get; }
        IKeyEventArgs LastKeyEvent     { get; }

        IKeyEventArgs LastKeyDownEvent_NoneVirtual { get; }
        IKeyEventArgs LastKeyEvent_NoneVirtual     { get; }
        void BeginInvoke(Action<IKeyEventArgs> action, DispatcherPriority priority = DispatcherPriority.Send);
        void BeginInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Send);
    }
}