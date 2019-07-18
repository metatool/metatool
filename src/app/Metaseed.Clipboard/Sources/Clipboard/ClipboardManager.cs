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
using Clipboard.ViewModels;
using Clipboard.Views;
using Metaseed.MetaKeyboard;
using Metaseed.NotifyIcon;
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
            set { __state = value; }
        }

        private          Channel          _currentChannel;
        private readonly ClipboardService _clipboard;
        private          PasteTips        __pasteTips;

        private PasteTips _pasteTips
        {
            get
            {
                if (__pasteTips != null) return __pasteTips;
                __pasteTips = new PasteTips();
                var ViewModel = new PasteTipsViewModel();
                ViewModel.DataEntries  = _clipboard.DataService.DataEntries;
                _pasteTips.DataContext = ViewModel;
                return __pasteTips;
            }
        }

        private CloseToken _pasteTipsCloseToken;

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
                    _state          = State.None;
                    _currentChannel = null;
                    e.GoToState     = new List<ICombination>();
                    return;
                }

                e.Handled = true;
                _state    = State.C;
            });

            registerKeys.ForEach(key =>
            {
                var register = CState.Then(key.With(Keys.LControlKey));
                register.Down(e =>
                {
                    e.Handled = true;

                    _currentChannel = Channel.GetRegister(key.ToString());
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
                    Console.WriteLine($"copy to {_currentChannel}");
                    _clipboard.CopyTo(_currentChannel);
                });
            });


            var VState = Keys.V.With(Keys.ControlKey);
            VState.Down(e =>
            {
                if (_state == State.CtrlUpPasting)
                {
                    _currentChannel = null;
                    _state          = State.None;
                    e.GoToState     = new List<ICombination>();
                    return;
                }

                _pasteTips.ViewModel.DataEntries = _clipboard.DataService.DataEntries;
                _pasteTipsCloseToken =
                    Notify.ShowMessage(_pasteTips, null, NotifyPosition.ActiveWindowCenter, true);
                e.Handled = true;
                _state    = State.V;
            });


            registerKeys.ForEach(key =>
            {
                var register = VState.Then(key.With(Keys.LControlKey));
                register.Down(e =>
                {
                    e.Handled                        = true;
                    _currentChannel                  = Channel.GetRegister(key.ToString());
                    _pasteTips.ViewModel.DataEntries = _currentChannel.GetContent();
                    _pasteTips.ViewModel.ChangeIsPasteAllState(_currentChannel);
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
                    Console.WriteLine($"paste from {_currentChannel}");
                    if (_currentChannel == null)
                    {
                        _clipboard.PasteFrom(_pasteTips.CurrentItemIndex);
                    }
                    else
                    {
                        _clipboard.PasteFrom(_currentChannel, _pasteTips.CurrentItemIndex);
                    }

                    _pasteTipsCloseToken?.Close();
                    _pasteTipsCloseToken = null;
                });
                _pasteTips.ViewModel.ResetIsPasteAll();
                _pasteTips.ViewModel.Channel = null;
            });

            var next = VState.Then(Keys.V.With(Keys.ControlKey));
            next.Down(e =>
            {
                e.Handled = true;
                _state    = State.CtrlUpPasting;

                e.BeginInvoke(() =>
                {
                    Console.WriteLine($"paste from next");
                    _pasteTips.Next();
                });
            });
            next.Up(e => { e.GoToState = new List<ICombination>() {VState}; });

            var last = VState.Then(Keys.C.With(Keys.ControlKey));
            last.Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() =>
                {
                    Console.WriteLine($"paste from previous");
                    _pasteTips.Previous();
                    //_clipboard.PasteFrom(-1);
                });
            });
            last.Up(e => { e.GoToState = new List<ICombination>() {VState}; });


            VState.Then(Keys.B.With(Keys.ControlKey)).Down(e =>
            {
                e.Handled = true;
                _pasteTipsCloseToken?.Close();
                e.BeginInvoke(() => { Messenger.Default.Send(new Message(), MessageIdentifiers.ShowPasteBarWindow); });
            });
            _clipboard = ServiceLocator.GetService<ClipboardService>();

            Keyboard.Hook();
        }
    }
}