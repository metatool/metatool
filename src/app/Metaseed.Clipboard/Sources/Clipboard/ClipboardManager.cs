using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Clipboard.ComponentModel.Messages;
using Clipboard.Core.Desktop.Models;
using Clipboard.Core.Desktop.Services;
using Clipboard.Shared.Services;
using GalaSoft.MvvmLight.Messaging;
using Metaseed.Input;
using Clipboard.ComponentModel.Messages;
using Message = Clipboard.ComponentModel.Messages.Message;

namespace Clipboard
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

        private State __state = State.None;

        private State _state
        {
            get { return __state; }
            set
            {
                __state = value;
            }
        }

        private Register         _currentRegister;
        private ClipboardService _clipboard;

        public ClipboardManager()
        {
            var registerKeys = new List<Keys>()
            {
                Keys.A, Keys.S, Keys.D, Keys.F, Keys.G
            };

            var CState = Keys.C.With(Keys.ControlKey);

            CState.Down(e =>
            {
                if (_state == State.CtrlUpCopying)
                {
                    _state = State.None;
                    _currentRegister = null;
                    e.GoToState = new List<ICombination>();
                    return;
                }

                e.Handled = true;
                _state = State.C;
            });

            registerKeys.ForEach(key =>
            {
                var register = CState.Then(key.With(Keys.LControlKey));
                register.Down(e =>
                {
                    e.Handled = true;

                    _currentRegister = Register.GetRegister(key.ToString());

                    if (!_currentRegister.IsAppend.HasValue) _currentRegister.IsAppend = false;
                    else if (!_currentRegister.IsAppend.GetValueOrDefault()) _currentRegister.IsAppend = true;

                    Console.WriteLine($"appending:{_currentRegister.IsAppend}");
                });
                register.Up(e => { e.GoToState = new List<ICombination>() {CState}; });
            });

            CState.Then(Keys.LControlKey).Up(e =>
            {
                // _state == State.None if triggered from real paste action.
                if (_state == State.None) return;
                _state = State.CtrlUpCopying;
                e.BeginInvoke(() =>
                {
                    Console.WriteLine($"copy to {_currentRegister}");
                    _clipboard.CopyTo(_currentRegister);
                });
            });


            var VState = Keys.V.With(Keys.ControlKey);
            VState.Down(e =>
            {
                if (_state == State.CtrlUpPasting)
                {
                    _currentRegister = null;
                    _state = State.None;
                    e.GoToState = new List<ICombination>();
                    return;
                }

                e.Handled = true;
                _state = State.V;
            });


            registerKeys.ForEach(key =>
            {
                var register = VState.Then(key.With(Keys.LControlKey));
                register.Down(e =>
                {
                    e.Handled = true;
                    _currentRegister = Register.GetRegister(key.ToString());
                });
                register.Up(e => { e.GoToState = new List<ICombination>() {VState}; });
            });

            VState.Then(Keys.LControlKey).Up(e =>
            {
                // _state == State.None if triggered from real paste action.
                if (_state == State.None) return;
                _state = State.CtrlUpPasting;
                e.BeginInvoke(() =>
                {
                    Console.WriteLine($"paste from {_currentRegister}");
                    _clipboard.PasteFrom(_currentRegister);
                });
            });

            VState.Then(Keys.V.With(Keys.ControlKey)).Down(e =>
            {
                e.Handled = true;
                _state = State.CtrlUpPasting;

                e.BeginInvoke(() =>
                {
                    Console.WriteLine($"paste from last");

                });
            });
            VState.Then(Keys.C.With(Keys.ControlKey)).Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() =>
                {
                    Console.WriteLine($"paste from previous");

                    _clipboard.PasteFrom(-1);
                });
            });

            VState.Then(Keys.B.With(Keys.ControlKey)).Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() =>
                {
                    Messenger.Default.Send(new Message(), MessageIdentifiers.ShowPasteBarWindow);
                });
            });
            _clipboard = ServiceLocator.GetService<ClipboardService>();

            Keyboard.Hook();
        }
    }
}