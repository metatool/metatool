using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Tests.Mocks;

public class MockKeyEventArgs : IKeyEventArgs
{
    private readonly MockKeyboardState _keyboardState;

    public MockKeyEventArgs(KeyCodes keyCode, KeyEventType eventType, MockKeyboardState? keyboardState = null)
    {
        KeyCode = keyCode;
        KeyData = keyCode;
        Key = new Key(keyCode);
        KeyEventType = eventType;
        _keyboardState = keyboardState ?? new MockKeyboardState();
    }

    public bool Handled { get; set; }
    public Key Key { get; }
    public KeyCodes KeyData { get; }
    public KeyCodes KeyCode { get; }
    public bool Alt => KeyboardState.IsDown(new Key(KeyCodes.Menu));
    public bool Control => KeyboardState.IsDown(new Key(KeyCodes.ControlKey));
    public bool Shift => KeyboardState.IsDown(new Key(KeyCodes.ShiftKey));
    public bool NoFurtherProcess { get; set; }
    public void DisableVirtualKeyHandlingInThisEvent() { }
    public int ScanCode => 0;
    public bool IsVirtual => false;
    public int Timestamp => Environment.TickCount;
    public bool IsKeyDown => KeyEventType == KeyEventType.Down;
    public KeyEventType KeyEventType { get; set; }
    public IKeyboardState KeyboardState => _keyboardState;
    public IKeyPath? PathToGo => null;
    public bool IsKeyUp => KeyEventType == KeyEventType.Up;
    public bool IsExtendedKey => false;
    public IKeyEventArgs LastKeyDownEvent => this;
    public IKeyEventArgs LastKeyEvent => this;
    public IKeyEventArgs LastKeyDownEvent_NoneVirtual => this;
    public IKeyEventArgs LastKeyEvent_NoneVirtual => this;
}
