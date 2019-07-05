using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Clipboard.Core.Desktop.Models;
using Clipboard.Core.Desktop.Services;
using Clipboard.Shared.Services;
using Metaseed.Input;

namespace Metaseed.MetaKeyboard
{
    public class ClipboardManager
    {
        private KeyStateMachine _stateMachine = new KeyStateMachine();

        enum State
        {
            None,
            C,
            V,
            CtrlUpCopying,
            CtrlUpPasting
        }

        private State            _state = State.None;
        private Register         _currentRegister;
        private ClipboardService _clipboard;

        public ClipboardManager()
        {
            var copy = Keys.C.With(Keys.ControlKey);

            copy.Down("", "", e =>
            {
                if (_state == State.CtrlUpCopying)
                {
                    _state = State.None;
                    return;
                }
                _state = State.C;
                e.Handled = true;
            });

            var paste = Keys.V.With(Keys.ControlKey);
            paste.Down("", "", e =>
            {
                if (_state == State.CtrlUpPasting)
                {
                    _state = State.None;
                    return;
                }

                _state = State.V;
                e.Handled = true;
            });

            var registerKeys = new List<Keys>()
            {
                Keys.A, Keys.S, Keys.D, Keys.F, Keys.G
            };

            copy.Then(Keys.ControlKey).Up("", "", ContolUp);

            registerKeys.ForEach(key =>
            {
                copy.Then(key.With(Keys.ControlKey)).Down("", "", e =>
                {
                    _currentRegister = Register.GetRegister(key.ToString());
                    e.Handled = true;
                });

                paste.Then(key.With(Keys.ControlKey)).Down("", "", e =>
                {
                    _currentRegister = Register.GetRegister(key.ToString());
                    e.Handled = true;
                });
            });


            void ControlUp(KeyEventArgsExt e)
            {
                if (_state == State.C)
                {
                    _state = State.CtrlUpCopying;
                    _clipboard.CopyTo(_currentRegister);
                }
                else if (_state == State.V)
                {
                    _state = State.CtrlUpPasting;
                    _clipboard.PasteFrom(_currentRegister);
                }

                _currentRegister = null;
            }
            Keys.LControlKey.Up("", "", ControlUp);
            _clipboard = ServiceLocator.GetService<ClipboardService>();

            Keyboard.Hook();
        }
    }
}