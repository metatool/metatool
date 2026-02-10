using Metatool.Input.MouseKeyHook.WinApi;
using Metatool.Service.MouseKey;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;

namespace Metatool.Input.MouseKeyHook.Implementation;

/// <summary>
///     Contains a snapshot of a keyboard state at certain moment and provides methods
///     of querying whether specific keys are pressed or locked.
/// </summary>
/// <remarks>
///     This class is basically a managed wrapper of GetKeyboardState API function
///     http://msdn.microsoft.com/en-us/library/ms646299
/// </remarks>
public partial class KeyboardState : IKeyboardState
{
    private static readonly MemoryMappedViewAccessor Accessor;

    static KeyboardState()
    {
        var m = MemoryMappedFile.CreateOrOpen("Metatool.HandledDownKeys", 256);
        Accessor = m.CreateViewAccessor(0, 256);
    }

    public static KeyboardState HandledDownKeys;
    private readonly byte[] _keyboardStateNative;

    private KeyboardState(byte[] keyboardStateNative)
    {
        _keyboardStateNative = keyboardStateNative;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (this != HandledDownKeys)
            sb.Append(HandledDownKeys.ToString() + "real: ");
        for (var i = 0; i < 256; i++)
        {
            var key = (KeyCodes)i;
            if (_keyboardStateNative[i] != 0)
            {
                if (IsDown(key)) sb.Append($"{key}↓ ");
                if (IsToggled(key)) sb.Append($"{key}~ ");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Makes a snapshot of a keyboard state to the moment of call and returns an
    ///     instance of <see cref="KeyboardState" /> class.
    /// </summary>
    /// <returns>An instance of <see cref="KeyboardState" /> class representing a snapshot of keyboard state at certain moment.</returns>
    public static KeyboardState Current()
    {
        var bytes = new byte[256];
        Accessor.ReadArray<byte>(0, bytes, 0, 256);
        HandledDownKeys = new KeyboardState(bytes);
        var keyboardStateNative = new byte[256];
        KeyboardNativeMethods.GetKeyboardState(keyboardStateNative);
        return new KeyboardState(keyboardStateNative);
    }

    /// <summary>
    ///     Makes a snapshot of a keyboard state using GetAsyncKeyState which queries
    ///     the actual hardware state rather than the message-based state.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Use this method when you need accurate key state outside the normal message pump timing,
    ///     such as when querying keyboard state from a posted action in a low-level keyboard hook.
    ///     </para>
    ///     <para>
    ///     <b>Why this exists:</b> The standard <see cref="Current"/> method uses GetKeyboardState/GetKeyState,
    ///     which return the keyboard state based on processed messages. In a low-level keyboard hook
    ///     (WH_KEYBOARD_LL), the hook callback fires BEFORE the WM_KEYUP/WM_KEYDOWN message is posted
    ///     to the message queue. If you post an action from the hook and query GetKeyState in that action,
    ///     the key may still appear "down" even after a KeyUp event because the message hasn't been
    ///     processed yet. GetAsyncKeyState queries the actual physical hardware state, bypassing this issue.
    ///     </para>
    ///     <para>
    ///     <b>Toggle keys:</b> For CapsLock, NumLock, ScrollLock, and Insert, we still use GetKeyState
    ///     to get the toggle state (ON/OFF). GetAsyncKeyState only reports whether the key is physically
    ///     pressed - it has no concept of toggle state. The low bit in GetAsyncKeyState means
    ///     "pressed since last call" (unreliable), NOT "toggled ON" like in GetKeyState.
    /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getasynckeystate
    ///     </para>
    /// </remarks>
    /// <returns>An instance of <see cref="KeyboardState" /> class representing the actual hardware keyboard state.</returns>
    public static KeyboardState CurrentAsync()
    {
        var bytes = new byte[256];
        Accessor.ReadArray<byte>(0, bytes, 0, 256);
        HandledDownKeys = new KeyboardState(bytes);
        var keyboardStateNative = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            var state = KeyboardNativeMethods.GetAsyncKeyState(i);
            // High-order bit set means key is currently down
            if ((state & 0x8000) != 0)
                keyboardStateNative[i] |= 0x80;
            // For toggle keys, we still need GetKeyState to get the toggle state
            if (i is (int)KeyCodes.CapsLock or (int)KeyCodes.NumLock or (int)KeyCodes.Scroll or (int)KeyCodes.Insert)
            {
                var toggleState = KeyboardNativeMethods.GetKeyState(i);
                if ((toggleState & 0x01) != 0)
                    keyboardStateNative[i] |= 0x01;
            }
        }
        return new KeyboardState(keyboardStateNative);
    }


    internal byte[] GetNativeState()
    {
        return _keyboardStateNative;
    }

    /// <summary>
    ///     Indicates whether specified key was down at the moment when snapshot was created or not.
    /// </summary>
    /// <param name="key">Key (corresponds to the virtual code of the key)</param>
    /// <returns><b>true</b> if key was down, <b>false</b> - if key was up.</returns>
    public bool IsDown(KeyCodes key)
    {
        if (this != HandledDownKeys)
        {
            if (HandledDownKeys.IsDown(key)) return true;
        }

        if ((int)key < 256) return IsDownRaw(key);
        if (key == KeyCodes.Alt) return IsDownRaw(KeyCodes.LMenu) || IsDownRaw(KeyCodes.RMenu);
        if (key == KeyCodes.Shift) return IsDownRaw(KeyCodes.LShiftKey) || IsDownRaw(KeyCodes.RShiftKey);
        if (key == KeyCodes.Control) return IsDownRaw(KeyCodes.LControlKey) || IsDownRaw(KeyCodes.RControlKey);
        return false;
    }

    public bool IsDown(Key key)
    {
        return key.Codes.Any(IsDown);
    }
    /// <summary>
    ///  This Function will return a Boolean as to whether the Key value passed in is Locked...
    /// </summary>
    public bool IsKeyLocked(KeyCodes keyVal)
    {
        if (keyVal is KeyCodes.Insert or KeyCodes.NumLock or KeyCodes.CapsLock or KeyCodes.Scroll)
        {
            int result = GetKeyState(keyVal);

            // If the high-order bit is 1, the key is down; otherwise, it is up.
            // If the low-order bit is 1, the key is toggled. A key, such as the CAPS LOCK key,
            // is toggled if it is turned on. The key is off and untoggled if the low-order bit is 0.
            // A toggle key's indicator light (if any) on the keyboard will be on when the key is toggled,
            // and off when the key is untoggled.

            // Toggle keys (only low bit is of interest).
            if (keyVal is KeyCodes.Insert or KeyCodes.CapsLock)
            {
                return (result & 0x1) != 0x0;
            }

            return (result & 0x8001) != 0x0;
        }

        // else - it's an un-lockable key.
        // Actually get the exception string from the system resource.
        throw new NotSupportedException("IsKeyLockedNumCapsScrollLockKeysSupportedOnly");
    }

    /// <summary>
    /// current down keys has some key not in the key's codes.
    /// Note: some of the key's codes may be not down.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsOtherDown(Key key)
    {
        if (key == Key.CtrlChord) key = Key.Ctrl;
        if (key == Key.AltChord) key = Key.Alt;
        if (key == Key.ShiftChord) key = Key.Shift;

        var downKeys = AllDownKeys.ToArray();
        return downKeys.Length > key.Codes.Count || downKeys.Any(k => !key.Codes.Contains(k));
    }

    public bool IsOtherDown(KeyCodes key)
    {
        if (key == KeyCodes.Alt) return IsOtherDown(Key.Alt);
        if (key == KeyCodes.Control) return IsOtherDown(Key.Ctrl);
        if (key == KeyCodes.Shift) return IsOtherDown(Key.Shift);

        var downKeys = AllDownKeys.ToArray();
        return downKeys.Length > 1 || (downKeys.Length == 1 && downKeys[0] != key);
    }

    public IEnumerable<KeyCodes> AllDownKeys
    {
        get
        {
            IEnumerable<KeyCodes> GetDownKeys()
            {
                for (var i = 0; i < _keyboardStateNative.Length; i++)
                {
                    if (GetHighBit(_keyboardStateNative[i]))
                        yield return (KeyCodes)i;
                }
            }

            return (this != HandledDownKeys
                ? HandledDownKeys.AllDownKeys.Concat(GetDownKeys())
                : GetDownKeys()).Distinct();
        }
    }

    public IEnumerable<Key> DownKeys => AllDownKeys.Select(k => new Key(k));


    public bool IsUp(KeyCodes key)
    {
        if (this != HandledDownKeys)
        {
            if (HandledDownKeys.IsDown(key)) return false;
        }

        if ((int)key < 256) return IsUpRaw(key);
        if (key == KeyCodes.Alt) return IsUpRaw(KeyCodes.LMenu) || IsUpRaw(KeyCodes.RMenu);
        if (key == KeyCodes.Shift) return IsUpRaw(KeyCodes.LShiftKey) || IsUpRaw(KeyCodes.RShiftKey);
        if (key == KeyCodes.Control) return IsUpRaw(KeyCodes.LControlKey) || IsUpRaw(KeyCodes.RControlKey);
        return false;
    }


    public bool IsUp(Key key)
    {
        return key.Codes.Any(IsUp);
    }

    internal void SetKeyUp(KeyCodes key)
    {
        var virtualKeyCode = (int)key;
        if (virtualKeyCode < 0 || virtualKeyCode > 255)
            throw new ArgumentOutOfRangeException("key", key, "The value must be between 0 and 255.");

        var v = _keyboardStateNative[virtualKeyCode] &= 0x7F;

        if (this == HandledDownKeys)
        {
            Accessor.Write(virtualKeyCode, v);
        }
    }

    internal void SetKeyDown(KeyCodes key)
    {
        var virtualKeyCode = (int)key;
        if (virtualKeyCode < 0 || virtualKeyCode > 255)
            throw new ArgumentOutOfRangeException("key", key, "The value must be between 0 and 255.");

        var v = _keyboardStateNative[virtualKeyCode] |= 0x80;

        if (this == HandledDownKeys)
        {
            Accessor.Write(virtualKeyCode, v);
        }
    }

    private bool IsUpRaw(KeyCodes key)
    {
        return !IsDownRaw(key);
    }

    private bool IsDownRaw(KeyCodes key)
    {
        var keyState = GetKeyState(key);

        //            var rawState = KeyboardNativeMethods.GetAsyncKeyState((UInt16)Keys.CapsLock);
        var isDown = GetHighBit(keyState);
        //            Console.WriteLine($"{key}-state:{isDown};R:{rawState < 0}");
        return isDown;
    }

    /// <summary>
    ///     Indicate weather specified key was toggled at the moment when snapshot was created or not.
    ///     The low-order bit is meaningless for non-toggle keys.
    /// </summary>
    /// <param name="key">Key (corresponds to the virtual code of the key)</param>
    /// <returns>
    ///     <b>true</b> if toggle key like (CapsLock, NumLocke, etc.) was on. <b>false</b> if it was off.
    ///     Ordinal (non toggle) keys return always false.
    /// </returns>
    public bool IsToggled(KeyCodes key)
    {
        if (key != KeyCodes.CapsLock && key != KeyCodes.NumLock && key != KeyCodes.Scroll && key != KeyCodes.Insert) return false;

        var keyState = GetKeyState(key);
        var isToggled = GetLowBit(keyState);
        return isToggled;
    }

    public bool IsToggled(Key key)
    {
        return key.Codes.Any(IsToggled);
    }

    /// <summary>
    ///     Indicates weather every of specified keys were down at the moment when snapshot was created.
    ///     The method returns false if even one of them was up.
    /// </summary>
    /// <param name="keys">Keys to verify whether they were down or not.</param>
    /// <returns><b>true</b> - all were down. <b>false</b> - at least one was up.</returns>
    public bool AreAllDown(IEnumerable<KeyCodes> keys)
    {
        return keys.All(IsDown);
    }

    public bool AreAllDown(IEnumerable<Key> keys)
    {
        return keys.All(IsDown);
    }

    public bool AreAllUp(IEnumerable<KeyCodes> keys)
    {
        return keys.All(IsUp);
    }

    public bool AreAllUp(IEnumerable<Key> keys)
    {
        return keys.All(IsUp);
    }

    private byte GetKeyState(KeyCodes key)
    {
        var virtualKeyCode = (int)key;
        if (virtualKeyCode < 0 || virtualKeyCode > 255)
            throw new ArgumentOutOfRangeException("key", key, "The value must be between 0 and 255.");
        return _keyboardStateNative[virtualKeyCode];
    }

    private static bool GetHighBit(byte value)
    {
        return value >> 7 != 0;
    }

    private static bool GetLowBit(byte value)
    {
        return (value & 1) != 0;
    }
}