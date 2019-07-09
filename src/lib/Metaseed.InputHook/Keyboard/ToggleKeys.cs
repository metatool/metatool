using System;
using System.Windows.Forms;
using System.Windows.Threading;
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
        private bool _valid;
        private IRemovable keyDownActionToken;
        private IRemovable keyUpActionToken;
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
                keyDownActionToken = _key.Down(e =>
                {
                    if (!_isAlwaysOn.HasValue) return;


                    if (_key == Keys.NumLock)
                    {
                        var on = Control.IsKeyLocked(_key);
                        if (on && !_isAlwaysOn.Value || !on && _isAlwaysOn.Value)
                        {
                            _valid = true;
                            return;
                        }


                        return;
                    }

                    if (_confirmAlwaysOnOffSate)
                    {
                        _confirmAlwaysOnOffSate = false;
                        var on = Control.IsKeyLocked(_key);
                        if (on && !_isAlwaysOn.Value || !on && _isAlwaysOn.Value)
                        {
                            _valid = true;
                            return;
                        }
                    }


                    handled = KeyboardState.HandledDownKeys.Add(_key);
                    e.Handled = true;
                }, $"Metaseed.AlwaysOnOff_{_key}_Down", "", false);
            if (keyUpActionToken == null)
                keyUpActionToken = _key.Up(e =>
                {
                    if (!_isAlwaysOn.HasValue) return;

                    if (_key == Keys.NumLock)
                    {
                        if (_valid)
                        {
                            _valid = false;
                            return;
                        }

                        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, (Action)(() =>
                            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)_key)));
                        return;
                    }

                    if (_valid)
                    {
                        _valid = false;
                        return;
                    }

                    if (handled)
                    {
                        handled = false;
                    }
                    e.Handled = true;
                }, $"Metaseed.AlwaysOnOff_{_key}_Up", "");
        }

        void RemoveHook()
        {
            keyDownActionToken?.Remove();
            keyDownActionToken = null;
            keyUpActionToken?.Remove();
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
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, (Action)(() =>
                            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)_key)
                        ));
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
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, (Action)(() =>

                            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)_key)
                        ));

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
