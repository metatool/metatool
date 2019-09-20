using System.Collections.Generic;
using System.Windows.Forms;
using Clipboard.ComponentModel.Messages;
using Clipboard.Core.Desktop.Models;
using Clipboard.Core.Desktop.Services;
using Clipboard.Shared.Services;
using GalaSoft.MvvmLight.Messaging;
using Metatool.Input;
using Clipboard.Views;
using Metatool.MetaKeyboard;
using Metatool.NotifyIcon;
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

            var cState = Keys.C.With(Keys.ControlKey);

            cState.Down(e =>
            {
                if (e.IsVirtual) return;

                e.Handled = true;

                CopyTips.ViewModel.SetData(_clipboard.DataService.DataEntries);
                _copyTipsCloseToken =
                    Notify.ShowMessage(CopyTips, null, NotifyPosition.ActiveWindowCenter, true);
            }, "Metatool.CopyToHistory", $"&Copy To Clipboard");

            cState.Up(e =>
            {
                if (e.IsVirtual)
                    e.GoToState = Keyboard.Root;
            });
            registerKeys.ForEach(key =>

            {
                var register = cState.Then(key.With(Keys.LControlKey));
                register.Down(e =>
                {
                    e.Handled       = true;
                    _currentChannel = Channel.GetChannel(key.ToString());
                    CopyTips.ViewModel.SetChannelData(_currentChannel);
                }, $"Metatool.CopyTo{key}", $"Copy to channel &{key}");
                register.Up(e => { e.GoToState = cState; });
            });
            var nextC = cState.Then(Keys.V.With(Keys.ControlKey));
            nextC.Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() => { CopyTips.Next(); });
            }, "Metatool.CopyToNextPosition", "Copy to next position");
            nextC.Up(e => { e.GoToState = cState; });

            var lastC = cState.Then(Keys.C.With(Keys.ControlKey));
            lastC.Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() => { CopyTips.Previous(); });
            }, "Metatool.CopyToPreviousPosition", "Copy to previous position");
            lastC.Up(e => { e.GoToState = cState; });


            cState.Then(Keys.LControlKey).Up(e =>
            {
                _copyTipsCloseToken?.Close();
                _copyTipsCloseToken = null;
                cState.Disabled     = true;
                e.BeginInvoke(() =>
                {
                    _clipboard.CopyTo(_currentChannel);
                    _currentChannel = null;
                    cState.Disabled =
                        false; // the copy action Ctrl+C keys handled async, so using IsVirtual at cState in case of Disabled = false happened before Ctrl+C keys process by message queen
                });
            }, "Metatool.CopyTakeEffect", "Do copy action");


            var vState = Keys.V.With(Keys.ControlKey);
            vState.Down(e =>
            {
                if (e.IsVirtual) return;

                PasteTips.ViewModel.SetData(_clipboard.DataService.DataEntries);
                _pasteTipsCloseToken =
                    Notify.ShowMessage(PasteTips, null, NotifyPosition.ActiveWindowCenter, true);
                e.Handled = true;
            }, "Metatool.PasteFromClipboardHistory", "&Paste from clipboard history");

            vState.Up(e =>
            {
                if (e.IsVirtual)
                    e.GoToState = Keyboard.Root;
            });

            registerKeys.ForEach(key =>
            {
                var register = vState.Then(key.With(Keys.LControlKey));
                register.Down(e =>
                {
                    e.Handled       = true;
                    _currentChannel = Channel.GetChannel(key.ToString());
                    PasteTips.ViewModel.SetChannelData(_currentChannel);
                }, $"Metatool.PasteFromChannel{key}", $"Paste from channel &{key}");
                register.Up(e => { e.GoToState = vState; });
            });

            vState.Then(Keys.LControlKey).Up(e =>
            {
                _pasteTipsCloseToken?.Close();
                _pasteTipsCloseToken = null;

                e.BeginInvoke(async () =>
                {
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

                });
            }, "Metatool.DoPasteAction", "Do paste action");

            var next = vState.Then(Keys.V.With(Keys.ControlKey));
            next.Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() => { PasteTips.Next(); });
            }, "Metatool.PasteFromNextPosition", "Paste from next position");
            next.Up(e => { e.GoToState = vState; });

            var last = vState.Then(Keys.C.With(Keys.ControlKey));
            last.Down(e =>
            {
                e.Handled = true;

                e.BeginInvoke(() => { PasteTips.Previous(); });
            }, "Metatool.PasteFromPreviousPosition", "Paste from previous position");
            last.Up(e => { e.GoToState = vState; });


            vState.Then(Keys.B.With(Keys.ControlKey)).Down(e =>
            {
                e.Handled = true;
                _pasteTipsCloseToken?.Close();
                e.BeginInvoke(() => { Messenger.Default.Send(new Message(), MessageIdentifiers.ShowPasteBarWindow); });
            }, "Metatool.ClipboardBrowseAndManagement", "&Browse and manage clipboard");
            _clipboard = ServiceLocator.GetService<ClipboardService>();

            Keyboard.Hook();
        }
    }
}
