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

        private          Channel          _currentChannel;
        private readonly ClipboardService _clipboard;
        private          PasteTips        _pasteTips;
        private          CopyTips         _copyTips;

        private PasteTips PasteTips
        {
            get
            {
                if (_pasteTips != null) return _pasteTips;
                _pasteTips = new PasteTips();
                return _pasteTips;
            }
        }

        private CloseToken _pasteTipsCloseToken;

        private CopyTips CopyTips
        {
            get
            {
                if (_copyTips != null) return _copyTips;
                _copyTips = new CopyTips();
                return _copyTips;
            }
        }

        private CloseToken _copyTipsCloseToken;

        public ClipboardManager()
        {
            var registerKeys = new List<Keys>()
            {
                Keys.A, Keys.S, Keys.D, Keys.F, Keys.G, Keys.Z
            };

            var CState = Keys.C.With(Keys.ControlKey);

            CState.Down(e =>
            {
                e.Handled = true;

                CopyTips.ViewModel.SetData(_clipboard.DataService.DataEntries);
                _copyTipsCloseToken =
                    Notify.ShowMessage(CopyTips, null, NotifyPosition.ActiveWindowCenter, true);
            });

            registerKeys.ForEach(key =>
            {
                var register = CState.Then(key.With(Keys.LControlKey));
                register.Down(e =>
                {
                    e.Handled = true;
                    _currentChannel = Channel.GetChannel(key.ToString());
                    CopyTips.ViewModel.SetChannelData(_currentChannel);
                });
                register.Up(e => { e.GoToState = CState; });
            });
            var nextC = CState.Then(Keys.V.With(Keys.ControlKey));
            nextC.Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() =>
                {
                    CopyTips.Next();
                });
            });
            nextC.Up(e => { e.GoToState = CState; });

            var lastC = CState.Then(Keys.C.With(Keys.ControlKey));
            lastC.Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() =>
                {
                    CopyTips.Previous();
                });
            });
            lastC.Up(e => { e.GoToState = CState; });


            CState.Then(Keys.LControlKey).Up(e =>
            {
                _copyTipsCloseToken?.Close();
                _copyTipsCloseToken = null;
                CState.Disabled     = true;
                CopyTips.ViewModel.ResetIsReplaceAll();

                e.BeginInvoke(() =>
                {
                    Console.WriteLine($"copy to {_currentChannel}");
                    _clipboard.CopyTo(_currentChannel);
                    _currentChannel = null;
                    CState.Disabled = false;
                });
            });


            var VState = Keys.V.With(Keys.ControlKey);
            VState.Down(e =>
            {
                PasteTips.ViewModel.SetData(_clipboard.DataService.DataEntries);
                _pasteTipsCloseToken =
                    Notify.ShowMessage(PasteTips, null, NotifyPosition.ActiveWindowCenter, true);
                e.Handled = true;
            });

            registerKeys.ForEach(key =>
            {
                var register = VState.Then(key.With(Keys.LControlKey));
                register.Down(e =>
                {
                    e.Handled       = true;
                    _currentChannel = Channel.GetChannel(key.ToString());
                    PasteTips.ViewModel.SetChannelData(_currentChannel);
                });
                register.Up(e => { e.GoToState = VState; });
            });

            VState.Then(Keys.LControlKey).Up(e =>
            {
                _pasteTipsCloseToken?.Close();
                _pasteTipsCloseToken = null;
                VState.Disabled      = true;
                PasteTips.ViewModel.ResetIsPasteAll();

                e.BeginInvoke(async () =>
                {
                    Console.WriteLine($"------------paste from {_currentChannel}");
                    if (_currentChannel == null)
                    {
                        await _clipboard.PasteFrom(PasteTips.CurrentItemIndex);
                    }
                    else
                    {
                        var channel = _currentChannel;
                        _currentChannel = null;
                        await _clipboard.PasteFrom(channel, PasteTips.CurrentItemIndex);
                    }

                    VState.Disabled = false;
                });
            });

            var next = VState.Then(Keys.V.With(Keys.ControlKey));
            next.Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() =>
                {
                    Console.WriteLine($"paste from next");
                    PasteTips.Next();
                });
            });
            next.Up(e => { e.GoToState = VState; });

            var last = VState.Then(Keys.C.With(Keys.ControlKey));
            last.Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() =>
                {
                    Console.WriteLine($"paste from previous");
                    PasteTips.Previous();
                });
            });
            last.Up(e => { e.GoToState = VState; });


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