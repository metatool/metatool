using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Metaseed.Clipboard;
using Metaseed.Input;

namespace Metaseed.MetaKeyboard
{
    public class ClipboardManager
    {
        private Clipboard.Clipboard _clipboard    = new Clipboard.Clipboard();
        private KeyStateMachine     _stateMachine = new KeyStateMachine();

        enum State
        {
            None,
            C,
            V,
            CtrlUpCopying,
            CtrlUpPasting
        }

        private State    _state = State.None;
        private Register _currentRegister;

        public ClipboardManager()
        {
            var copy = Keys.C.With(Keys.ControlKey);

            copy.Down("", "", e =>
            {
                if (_state == State.CtrlUpCopying) return;
                _state = State.C;
                e.Handled = true;
            });

            var paste = Keys.V.With(Keys.ControlKey);
            paste.Down("", "", e =>
            {
                if (_state == State.CtrlUpPasting) return;
                _state = State.V;
                e.Handled = true;
            });

            copy.Then(Keys.A.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.A;
                e.Handled = true;
            });
            copy.Then(Keys.S.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.S;
                e.Handled = true;
            });
            copy.Then(Keys.D.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.D;
                e.Handled = true;
            });
            copy.Then(Keys.F.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.F;
                e.Handled = true;
            });
            copy.Then(Keys.G.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.G;
                e.Handled = true;
            });
            paste.Then(Keys.A.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.A;
                e.Handled = true;
            });
            paste.Then(Keys.S.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.S;
                e.Handled = true;
            });
            paste.Then(Keys.D.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.D;
                e.Handled = true;
            });
            paste.Then(Keys.F.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.F;
                e.Handled = true;
            });
            paste.Then(Keys.G.With(Keys.ControlKey)).Down("", "", e =>
            {
                _currentRegister = _clipboard.G;
                e.Handled = true;
            });

            Keys.LControlKey.Up("", "", e =>
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
                _state = State.None;
                _currentRegister = null;
            });


        }
    }
}