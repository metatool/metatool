using System;
using System.Windows.Forms;
using System.Windows.Threading;
using Metatool.Command;
using Metatool.Service;
using Metatool.WindowsInput.Native;

namespace Metatool.Input
{

    public class ToggleKey : IToggleKey
    {
        private readonly Key _key;
        private bool? _isAlwaysOn;
        private bool _confirmAlwaysOnOffSate;
        private bool _valid;
        private IKeyCommand _keyCommandDownActionToken;
        private IKeyCommand _keyCommandUpActionToken;
        internal ToggleKey(Key key)
        {
            _key = key;
        }

        public ToggleKeyState State
        {
            get
            {
                if (_isAlwaysOn.HasValue) return _isAlwaysOn.Value ? ToggleKeyState.AlwaysOn : ToggleKeyState.AlwaysOff;

                return Control.IsKeyLocked((Keys)_key) ? ToggleKeyState.On : ToggleKeyState.Off;
            }

        }

        void InstallHook()
        {
            if (_keyCommandDownActionToken == null)
                _keyCommandDownActionToken = _key.OnDown(e =>
                {
                    if (!_isAlwaysOn.HasValue) return;


                    if (_key == Keys.NumLock)
                    {
                        var on = Control.IsKeyLocked((Keys)_key);
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
                        var on = Control.IsKeyLocked((Keys)_key);
                        if (on && !_isAlwaysOn.Value || !on && _isAlwaysOn.Value)
                        {
                            _valid = true;
                            return;
                        }
                    }

                    e.Handled = true;
                }, e=> !e.IsVirtual);
            if (_keyCommandUpActionToken == null)
                _keyCommandUpActionToken = _key.OnUp(e =>
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
                            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)(Keys)_key)));
                        return;
                    }

                    if (_valid)
                    {
                        _valid = false;
                        return;
                    }

                    e.Handled = true;
                }, e=>!e.IsVirtual);
        }

        void RemoveHook()
        {
            _keyCommandDownActionToken?.Remove();
            _keyCommandDownActionToken = null;
            _keyCommandUpActionToken?.Remove();
            _keyCommandUpActionToken = null;
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
                            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)(Keys)_key)
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

                            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)(Keys)_key)
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
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)(Keys)_key);
                    break;
                case ToggleKeyState.AlwaysOff:
                    _isAlwaysOn = null;
                    break;
                case ToggleKeyState.AlwaysOn:
                    _isAlwaysOn = null;
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)(Keys)_key);
                    break;
            }

        }
        public void On()
        {
            RemoveHook();
            switch (State)
            {
                case ToggleKeyState.Off:
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)(Keys)_key);
                    break;
                case ToggleKeyState.On:
                    break;
                case ToggleKeyState.AlwaysOff:
                    _isAlwaysOn = null;
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)(Keys)_key);
                    break;
                case ToggleKeyState.AlwaysOn:
                    _isAlwaysOn = null;
                    break;
            }

        }

    }
}
