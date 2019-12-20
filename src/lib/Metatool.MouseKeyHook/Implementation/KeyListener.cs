using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Metatool.Input.MouseKeyHook.WinApi;
using Metatool.Service;
using Microsoft.Extensions.Logging;

namespace Metatool.Input.MouseKeyHook.Implementation
{
    internal abstract class KeyListener : BaseListener, IKeyboardEvents
    {
        private ILogger _logger;

        protected KeyListener(Subscribe subscribe)
            : base(subscribe)
        {
            _logger = Services.Get<ILogger<KeyListener>>();
        }

        public event KeyEventHandler      KeyDown;
        public event KeyPressEventHandler KeyPress;
        public event KeyEventHandler      KeyUp;

        public void InvokeKeyDown(IKeyEventArgs e)
        {
            var handler = KeyDown;
            if (DisableDownEvent)
            {
                _logger.LogDebug("this KeyUp event disabled");
                return;
            }
            if (handler == null || !e.IsKeyDown || e.Handled)
                return;
            handler(this, e);
        }

        public void InvokeKeyPress(KeyPressEventArgsExt e)
        {
            var handler = KeyPress;
            if (DisablePressEvent)
            {
                _logger.LogDebug("this KeyPress event disabled");
                return;
            }

            if (handler == null || e.Handled || e.IsNonChar)

                return;

            handler(this, e);
            _logger.LogDebug(new String('\t', _indentCounter) + e.ToString());
        }

        public void InvokeKeyUp(IKeyEventArgs e)
        {
            var handler = KeyUp;


            if (handler == null || !e.IsKeyUp || e.Handled)
                return;



            if (DisableUpEvent)
            {
                _logger.LogDebug("this KeyUp event disabled");
                return;
            }

            handler(this, e);

            if (KeyboardState.HandledDownKeys.IsDown(e.KeyCode))
            {
                KeyboardState.HandledDownKeys.SetKeyUp(e.KeyCode);
            }
        }

        private int _indentCounter = 0;

        public bool HandleVirtualKey { get; set; } = true;

        private bool _disableDownEvent;
        private bool _disableUpEvent;
        private bool _disablePressEvent;

        public bool DisableDownEvent
        {
            get => _disableDownEvent;

            set
            {
                _logger.LogDebug($"{nameof(DisableDownEvent)} = {value}");
                _disableDownEvent = value;
            }
        }

        public bool DisableUpEvent
        {
            get => _disableUpEvent;

            set
            {
                _logger.LogDebug($"{nameof(DisableUpEvent)} = {value}");
                _disableUpEvent = value;
            }
        }

        public bool DisablePressEvent
        {
            get => _disablePressEvent;
            set
            {
                _logger.LogDebug($"{nameof(DisablePressEvent)} = {value}");
                _disablePressEvent = value;
            }
        }

        protected override bool Callback(CallbackData data)
        {
            var args = GetDownUpEventArgs(data);
            if (Disable)
            {
                _logger.LogDebug('\t' + "NotHandled " + args.ToString());
                return true;
            }

            var argExt = args as KeyEventArgsExt;
            argExt.listener = this;
            if (args.IsVirtual && !HandleVirtualKey)
            {
                _logger.LogDebug('\t' + "NotHandled " + args.ToString());
                return true;
            }

            _logger.LogDebug(new String('\t', _indentCounter++) + "→" + args.ToString());
            InvokeKeyDown(args);

            var pressEventArgs = GetPressEventArgs(data, args).ToList();
            foreach (var pressEventArg in pressEventArgs)
                InvokeKeyPress(pressEventArg);
            InvokeKeyUp(args);
            _logger.LogDebug(new String('\t', --_indentCounter) + "←" + args.ToString());
            if (argExt.HandleVirtualKeyBackup.HasValue)
            {
                HandleVirtualKey = argExt.HandleVirtualKeyBackup.Value;
                _logger.LogDebug($"HandleVirtualKey={HandleVirtualKey}");
            }

            return !args.Handled && !pressEventArgs.Any(e => e.Handled);
        }

        protected abstract IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data, IKeyEventArgs arg);
        protected abstract IKeyEventArgs GetDownUpEventArgs(CallbackData data);
    }
}