using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WindowsInput.Native;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public enum ToggleKeyState
    {
        On, Off, AlwaysOn, AlwaysOff
    }
    public class ToggleKeys
    {
        public static ToggleKeys NumLock = new ToggleKeys(Keys.NumLock);
        public static ToggleKeys CapsLock = new ToggleKeys(Keys.CapsLock);
        public static ToggleKeys ScrollLock = new ToggleKeys(Keys.Scroll);
        public static ToggleKeys Insert = new ToggleKeys(Keys.Insert);

        private readonly Keys _key;
        private bool? _isAlwaysOn;
        private bool _confirmAlwaysOnOffSate;
        private bool handled;

        private IDisposable keyDownActionToken;
        private IDisposable keyUpActionToken;
        private ToggleKeys(Keys key)
        {
            _key = key;
        }

        public ToggleKeyState State
        {
            get
            {
                if (_isAlwaysOn.HasValue) return _isAlwaysOn.Value ? ToggleKeyState.AlwaysOn : ToggleKeyState.AlwaysOff;

                return Control.IsKeyLocked(_key) ? ToggleKeyState.On : ToggleKeyState.Off;
            }

        }

        void InstallHook()
        {
            if (keyDownActionToken == null)
                keyDownActionToken = _key.Down($"Metaseed.AlwaysOnOff_{_key}_Down", "", e =>
                {

                    if (!_isAlwaysOn.HasValue) return;

                    if (_confirmAlwaysOnOffSate)
                    {
                        _confirmAlwaysOnOffSate = false;
                        var on = Control.IsKeyLocked(_key);
                        if (on && !_isAlwaysOn.Value || !on && _isAlwaysOn.Value)
                            return;
                    }

                    handled = KeyboardState.OnToggleKeys.Add(_key);
                    e.Handled = true;
                });
            if (keyUpActionToken == null)
                keyUpActionToken = _key.Up($"Metaseed.AlwaysOnOff_{_key}_Up", "", e =>
                {
                    if (!_isAlwaysOn.HasValue) return;
                    if (handled) KeyboardState.OnToggleKeys.Remove(_key);
                    e.Handled = true;
                });
        }

        void RemoveHook()
        {
            keyDownActionToken?.Dispose();
            keyDownActionToken = null;
            keyUpActionToken?.Dispose();
            keyUpActionToken = null;
        }

        public void AlwaysOn()
        {
            InstallHook();

            switch (State)
            {
                case ToggleKeyState.Off:
                case ToggleKeyState.AlwaysOff:
                    _isAlwaysOn = true;
                    _confirmAlwaysOnOffSate = true;
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)_key);
                    break;
                case ToggleKeyState.On:
                    _isAlwaysOn = true;
                    break;
                case ToggleKeyState.AlwaysOn:
                    break;
            }
        }
        public void AlwaysOff()
        {
            InstallHook();

            switch (State)
            {
                case ToggleKeyState.AlwaysOff:
                    break;
                case ToggleKeyState.On:
                case ToggleKeyState.AlwaysOn:
                    _confirmAlwaysOnOffSate = true;
                    _isAlwaysOn = false;
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)_key);
                    break;
                case ToggleKeyState.Off:
                    _isAlwaysOn = false;
                    break;
            }

        }

        public void Off()
        {
            RemoveHook();
            switch (State)
            {
                case ToggleKeyState.Off:
                    break;
                case ToggleKeyState.On:
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)_key);
                    break;
                case ToggleKeyState.AlwaysOff:
                    _isAlwaysOn = null;
                    break;
                case ToggleKeyState.AlwaysOn:
                    _isAlwaysOn = null;
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)_key);
                    break;
            }

        }
        public void On()
        {
            RemoveHook();
            switch (State)
            {
                case ToggleKeyState.Off:
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)_key);
                    break;
                case ToggleKeyState.On:
                    break;
                case ToggleKeyState.AlwaysOff:
                    _isAlwaysOn = null;
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)_key);
                    break;
                case ToggleKeyState.AlwaysOn:
                    _isAlwaysOn = null;
                    break;
            }

        }

    }
}
