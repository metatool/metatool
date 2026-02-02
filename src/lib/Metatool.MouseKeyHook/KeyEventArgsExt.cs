using System;
using System.Runtime.InteropServices;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Input.MouseKeyHook.WinApi;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

/// <summary>
///     Provides extended argument data for the <see cref='KeyListener.KeyDown' /> or
///     <see cref='KeyListener.KeyUp' /> event.
/// </summary>
public class KeyEventArgsExt(KeyCodes keyData) : IKeyEventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IKeyEventArgs" /> class.
    /// </summary>
    /// <param name="keyData"></param>
    internal KeyEventArgsExt(KeyCodes keyData, int scanCode, int timestamp, bool isKeyDown, bool isKeyUp,
        bool isExtendedKey, IKeyEventArgs lastKeyEvent, IKeyboardState keyboardState):this(keyData)
    {
        ScanCode = scanCode;
        Timestamp = timestamp;
        IsKeyDown = isKeyDown;
        IsKeyUp = isKeyUp;
        KeyEventType = isKeyDown ? KeyEventType.Down : KeyEventType.Up;
        IsExtendedKey = isExtendedKey;

        LastKeyEvent = lastKeyEvent;
        LastKeyDownEvent = lastKeyEvent.LastKeyDownEvent;
        LastKeyDownEvent_NoneVirtual = lastKeyEvent.LastKeyDownEvent_NoneVirtual;
        LastKeyEvent_NoneVirtual = lastKeyEvent.LastKeyEvent_NoneVirtual;

        KeyboardState = keyboardState;
    }

    internal KeyEventArgsExt Copy()
    {
        return new KeyEventArgsExt(KeyData, ScanCode, Timestamp, IsKeyDown, IsKeyUp, IsExtendedKey, LastKeyEvent,
            KeyboardState);
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj is not KeyEventArgsExt arg) return false;
        return KeyData == arg.KeyData && ScanCode == arg.ScanCode && Timestamp == arg.Timestamp &&
               IsKeyDown == arg.IsKeyDown && IsKeyUp == arg.IsKeyUp;
    }
    // no mem leak
    public void ClearLastEventLink()
    {
        LastKeyDownEvent = null;
        LastKeyEvent = null;
        LastKeyDownEvent_NoneVirtual = null;
        LastKeyEvent_NoneVirtual = null;
    }

    //static Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

    public IKeyPath PathToGo { get; internal set; }

    public KeyCodes KeyData { get; } = keyData;

    /// <summary>
    ///  Gets the keyboard code for a <see cref="Control.KeyDown"/> or
    /// <see cref="Control.KeyUp"/> event.
    /// </summary>
    public KeyCodes KeyCode
    {
        get
        {
            var keyGenerated = KeyData & KeyCodes.KeyCode;

            // since Keys can be discontiguous, keeping Enum.IsDefined.
            if (!Enum.IsDefined(keyGenerated))
            {
                return KeyCodes.None;
            }

            return keyGenerated;
        }
    }

    /// <summary>
    ///     The hardware scan code.
    /// </summary>
    public int ScanCode { get; }

    private Key _key;
    public Key Key => _key ??= new Key(KeyData);

    /// <summary>
	/// is it from the keyboard simulator?
	/// </summary>
	public bool IsVirtual { get; private set; }

    /// <summary>
    ///     The system tick count of when the event occurred.
    /// </summary>
    public int Timestamp { get; }

    /// <summary>
    ///     True if event signals key down..
    /// </summary>
    public bool IsKeyDown { get; }

    public IKeyEventArgs LastKeyDownEvent { get; private set; }
    public IKeyEventArgs LastKeyEvent { get; private set; }

    public IKeyEventArgs LastKeyDownEvent_NoneVirtual { get; private set; }
    public IKeyEventArgs LastKeyEvent_NoneVirtual { get; private set; }
    internal KeyListener listener;
    internal bool? HandleVirtualKeyBackup;
    public virtual bool Shift => (KeyData & KeyCodes.Shift) == KeyCodes.Shift;

    public virtual bool Alt => (KeyData & KeyCodes.Alt) == KeyCodes.Alt;

    public bool Control => (KeyData & KeyCodes.Control) == KeyCodes.Control;

    public void DisableVirtualKeyHandlingInThisEvent()
    {
        HandleVirtualKeyBackup = listener.HandleVirtualKey;
        listener.HandleVirtualKey = false;
    }

    /// <summary>
    ///     True if event signals key up.
    /// </summary>
    public bool IsKeyUp { get; }

    /// <summary>
    ///     beyond the basic typing keys (letters, numbers, and common punctuation)
    /// These keys don’t usually produce characters directly but perform control, navigation, or system functions. note: LCtrl, LAlt is not extend key. LCtrl and RCtrl with same scan code 0x1D, the RCtrl with E0(Extended Flag) set.
    /// Extended keys often don’t map to ASCII characters, They’re detected via scan codes or virtual key codes. Examples: arrow keys, function keys, Ctrl/Alt combinations
    /// </summary>
    public bool IsExtendedKey { get; }

    /// <summary>
    /// is assigned when climbing the tree
    /// </summary>
    public KeyEventType KeyEventType { get; set; }

    private bool _handled;
    public new bool Handled
    {
        get => _handled;
        set
        {
            if (IsKeyDown && value) MouseKeyHook.Implementation.KeyboardState.HandledDownKeys.SetKeyDown(KeyCode);

            _handled = value;
        }
    }

    public bool NoFurtherProcess { get; set; }
    public IKeyboardState KeyboardState { get; }

    public override string ToString()
    {
        var time = DateTime.Now;
        time = time.AddMilliseconds(Timestamp - Environment.TickCount);
        var keyEvent = IsKeyUp ? "Up" : "Down";
        return
            $"{time:hh:mm:ss.fff} {KeyCode,-16} {keyEvent,-6}Handled:{Handled,-8} IsVirtual: {IsVirtual,-8} Scan:0x{ScanCode,-8:X} Extended:{IsExtendedKey}  State: {KeyboardState}";
    }

    private static KeyEventArgsExt _lastKeyEventGlobalBuffer = new(KeyCodes.None);
    private static KeyEventArgsExt _lastKeyEventApp = new(KeyCodes.None);

    internal static IKeyEventArgs FromRawDataApp(CallbackData data)
    {
        var wParam = data.WParam;
        var lParam = data.LParam;

        //http://msdn.microsoft.com/en-us/library/ms644984(v=VS.85).aspx

        const uint maskKeydown = 0x40000000; // for bit 30
        const uint maskKeyup = 0x80000000; // for bit 31
        const uint maskExtendedKey = 0x1000000;  // for bit 24

        var timestamp = Environment.TickCount;

        var flags = (uint)lParam.ToInt64();

        //bit 30 Specifies the previous key state. The value is 1 if the key is down before the message is sent; it is 0 if the key is up.
        var wasKeyDown = (flags & maskKeydown) > 0;
        //bit 31 Specifies the transition state. The value is 0 if the key is being pressed and 1 if it is being released.
        var isKeyReleased = (flags & maskKeyup) > 0;
        //bit 24 Specifies the extended key state. The value is 1 if the key is an extended key, otherwise the value is 0.
        var isExtendedKey = (flags & maskExtendedKey) > 0;


        var keyData = AppendModifierStates((KeyCodes)wParam);
        var scanCode = (int)(((flags & 0x1_0000) | (flags & 0x2_0000) | (flags & 0x4_0000) | (flags & 0x8_0000) |
                               (flags & 0x10_0000) | (flags & 0x20_0000) | (flags & 0x40_0000) | (flags & 0x80_0000)) >>
                              16);

        var isKeyDown = !isKeyReleased;
        var isKeyUp = wasKeyDown && isKeyReleased;

        var r = new KeyEventArgsExt(keyData, scanCode, timestamp, isKeyDown, isKeyUp, isExtendedKey,
            _lastKeyEventApp,
            MouseKeyHook.Implementation.KeyboardState.Current());
        var copy = r.Copy();
        _lastKeyEventApp.ClearLastEventLink();

        _lastKeyEventApp = copy;
        if (isKeyDown) _lastKeyEventApp.LastKeyDownEvent = r;

        return r;
    }

    internal static IKeyEventArgs FromRawDataGlobal(CallbackData data)
    {
        var wParam = data.WParam;
        var lParam = data.LParam;
        var keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct))!;

        var keyData = AppendModifierStates((KeyCodes)keyboardHookStruct.VirtualKeyCode);
        var isVirtual = (keyboardHookStruct.ExtraInfo & 0x01) == 0x01;

        var keyCode = (int)wParam;
        var isKeyDown = keyCode == Messages.WM_KEYDOWN || keyCode == Messages.WM_SYSKEYDOWN;
        var isKeyUp = keyCode == Messages.WM_KEYUP || keyCode == Messages.WM_SYSKEYUP;

        const uint maskExtendedKey = 0x1;
        var isExtendedKey = (keyboardHookStruct.Flags & maskExtendedKey) > 0;

        var r = new KeyEventArgsExt(keyData, keyboardHookStruct.ScanCode, keyboardHookStruct.Time, isKeyDown,
            isKeyUp, isExtendedKey, _lastKeyEventGlobalBuffer, MouseKeyHook.Implementation.KeyboardState.Current());
        var copy = r.Copy(); // create a copy of r and then set. with copy, we need to override equal
        // no mem leak
        _lastKeyEventGlobalBuffer.ClearLastEventLink();

        _lastKeyEventGlobalBuffer = copy;
        if (isKeyDown) _lastKeyEventGlobalBuffer.LastKeyDownEvent = r;// if not copy, LastKeyDownEvent is right for current keydown
        if (!isVirtual) _lastKeyEventGlobalBuffer.LastKeyEvent_NoneVirtual = r;
        if (isKeyDown && !isVirtual) _lastKeyEventGlobalBuffer.LastKeyDownEvent_NoneVirtual = r;

        r.IsVirtual = isVirtual;
        return r;
    }

    // # It is not possible to distinguish Keys.LControlKey and Keys.RControlKey when they are modifiers
    // Check for Keys.Control instead
    // Same for Shift and Alt(Menu)
    // See more at http://www.tech-archive.net/Archive/DotNet/microsoft.public.dotnet.framework.windowsforms/2008-04/msg00127.html #

    // A shortcut to make life easier
    private static bool CheckModifier(int vKey)
    {
        return (KeyboardNativeMethods.GetKeyState(vKey) & 0x8000) > 0;
    }

    private static KeyCodes AppendModifierStates(KeyCodes keyData)
    {
        // Is Control being held down?
        var control = CheckModifier(KeyboardNativeMethods.VK_CONTROL);
        // Is Shift being held down?
        var shift = CheckModifier(KeyboardNativeMethods.VK_SHIFT);
        // Is Alt being held down?
        var alt = CheckModifier(KeyboardNativeMethods.VK_MENU);

        // Windows keys
        // # combine LWin and RWin key with other keys will potentially corrupt the data
        // notable F5 | Keys.LWin == F12, see https://globalmousekeyhook.codeplex.com/workitem/1188
        // and the KeyEventArgs.KeyData don't recognize combined data either

        // Function (Fn) key
        // # CANNOT determine state due to conversion inside keyboard
        // See http://en.wikipedia.org/wiki/Fn_key#Technical_details #

        return keyData |
               (control ? KeyCodes.Control : KeyCodes.None) |
               (shift ? KeyCodes.Shift : KeyCodes.None) |
               (alt ? KeyCodes.Alt : KeyCodes.None);
    }
}