



using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Metatool.Input.MouseKeyHook.WinApi;

namespace Metatool.Input.MouseKeyHook.Implementation
{
    /// <summary>
    ///     Contains a snapshot of a keyboard state at certain moment and provides methods
    ///     of querying whether specific keys are pressed or locked.
    /// </summary>
    /// <remarks>
    ///     This class is basically a managed wrapper of GetKeyboardState API function
    ///     http://msdn.microsoft.com/en-us/library/ms646299
    /// </remarks>
    public class KeyboardState: IKeyboardState
    {
        private static MemoryMappedViewAccessor accessor;

        static KeyboardState()
        {
            var m = MemoryMappedFile.CreateOrOpen("Metatool.HandledDownKeys", 256);
            accessor = m.CreateViewAccessor(0, 256);
        }

        public static KeyboardState HandledDownKeys;
        private       byte[]        _keyboardStateNative;

        private KeyboardState(byte[] keyboardStateNative)
        {
            this._keyboardStateNative = keyboardStateNative;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (this != HandledDownKeys)
                sb.Append(HandledDownKeys.ToString() + "|");
            for (var i = 0; i < 256; i++)
            {
                 var key = (Keys) i;
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
        public static KeyboardState GetCurrent()
        {
            var bytes = new byte[256];
            accessor.ReadArray<byte>(0, bytes, 0, 256);
            HandledDownKeys = new KeyboardState(bytes);
            var keyboardStateNative = new byte[256];
            KeyboardNativeMethods.GetKeyboardState(keyboardStateNative);
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
        public bool IsDown(Keys key)
        {
            if (this != HandledDownKeys)
            {
                if (HandledDownKeys.IsDown(key)) return true;
            }

            if ((int) key < 256) return IsDownRaw(key);
            if (key       == Keys.Alt) return IsDownRaw(Keys.LMenu)           || IsDownRaw(Keys.RMenu);
            if (key       == Keys.Shift) return IsDownRaw(Keys.LShiftKey)     || IsDownRaw(Keys.RShiftKey);
            if (key       == Keys.Control) return IsDownRaw(Keys.LControlKey) || IsDownRaw(Keys.RControlKey);
            return false;
        }

        public bool IsOtherDown(Key key)
        {
            if (key == Key.CtrlChord) key  = Key.Ctrl;
            if (key == Key.AltChord) key   = Key.Alt;
            if (key == Key.ShiftChord) key = Key.Shift;

            var downKeys = DownKeys.ToArray();
            return downKeys.Length > key.Codes.Count || downKeys.Any(k => !key.Codes.Contains(k));
        }

        public bool IsOtherDown(Keys key)
        {
            if (key == Keys.Alt) return IsOtherDown(Key.Alt);
            if (key == Keys.Control) return IsOtherDown(Key.Ctrl);
            if (key == Keys.Shift) return IsOtherDown(Key.Shift);

            var downKeys = DownKeys.ToArray();
            return downKeys.Length > 1 || (downKeys.Length == 1 && downKeys[0] != key);
        }

        public IEnumerable<Keys> DownKeys
        {
            get
            {
                IEnumerable<Keys> getDownKeys()
                {
                    for (var i = 0; i < _keyboardStateNative.Length; i++)
                    {
                        if (GetHighBit(_keyboardStateNative[i]))
                            yield return (Keys) i;
                    }
                }

                return this != HandledDownKeys
                    ? HandledDownKeys.DownKeys.Concat(getDownKeys())
                    : getDownKeys();
            }
        }

        public bool IsDown(Key key)
        {
            return key.Codes.Any(IsDown);
        }

        public bool IsUp(Keys key)
        {
            if (this != HandledDownKeys)
            {
                if (HandledDownKeys.IsDown(key)) return false;
            }

            if ((int) key < 256) return IsUpRaw(key);
            if (key       == Keys.Alt) return IsUpRaw(Keys.LMenu)           || IsUpRaw(Keys.RMenu);
            if (key       == Keys.Shift) return IsUpRaw(Keys.LShiftKey)     || IsUpRaw(Keys.RShiftKey);
            if (key       == Keys.Control) return IsUpRaw(Keys.LControlKey) || IsUpRaw(Keys.RControlKey);
            return false;
        }

        public bool IsUp(Key key)
        {
            return key.Codes.Any(IsUp);
        }

        internal void SetKeyUp(Keys key)
        {
            var virtualKeyCode = (int) key;
            if (virtualKeyCode < 0 || virtualKeyCode > 255)
                throw new ArgumentOutOfRangeException("key", key, "The value must be between 0 and 255.");

            var v = _keyboardStateNative[virtualKeyCode] &= 0x7F;

            if (this == HandledDownKeys)
            {
                accessor.Write(virtualKeyCode, v);
            }
        }

        internal void SetKeyDown(Keys key)
        {
            var virtualKeyCode = (int) key;
            if (virtualKeyCode < 0 || virtualKeyCode > 255)
                throw new ArgumentOutOfRangeException("key", key, "The value must be between 0 and 255.");

            var v = _keyboardStateNative[virtualKeyCode] |= 0x80;

            if (this == HandledDownKeys)
            {
                accessor.Write(virtualKeyCode, v);
            }
        }

        private bool IsUpRaw(Keys key)
        {
            return !IsDownRaw(key);
        }

        private bool IsDownRaw(Keys key)
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
        public bool IsToggled(Keys key)
        {
            if (key != Keys.CapsLock && key != Keys.NumLock && key != Keys.Scroll && key != Keys.Insert) return false;

            var keyState  = GetKeyState(key);
            var isToggled = GetLowBit(keyState);
            return isToggled;

        }

        /// <summary>
        ///     Indicates weather every of specified keys were down at the moment when snapshot was created.
        ///     The method returns false if even one of them was up.
        /// </summary>
        /// <param name="keys">Keys to verify whether they were down or not.</param>
        /// <returns><b>true</b> - all were down. <b>false</b> - at least one was up.</returns>
        public bool AreAllDown(IEnumerable<Keys> keys)
        {
            return keys.All(IsDown);
        }

        public bool AreAllDown(IEnumerable<Key> keys)
        {
            return keys.All(IsDown);
        }

        public bool AreAllUp(IEnumerable<Keys> keys)
        {
            return keys.All(IsUp);
        }

        public bool AreAllUp(IEnumerable<Key> keys)
        {
            return keys.All(IsUp);
        }

        private byte GetKeyState(Keys key)
        {
            var virtualKeyCode = (int) key;
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
}
