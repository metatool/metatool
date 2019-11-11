using System;
using System.Collections.Generic;
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
            if (handler == null || e.Handled || !e.IsKeyDown)
                return;
            handler(this, e);
        }

        public void InvokeKeyPress(KeyPressEventArgsExt e)
        {
            var handler = KeyPress;
            if (handler == null || e.Handled || e.IsNonChar)
                return;
            handler(this, e);
            _logger.LogInformation(new String('\t', _indentCounter) + e.ToString());
        }

        public void InvokeKeyUp(IKeyEventArgs e)
        {
            var handler = KeyUp;
            if (handler == null || e.Handled || !e.IsKeyUp)
                return;
            if (KeyboardState.HandledDownKeys.IsDown(e.KeyCode))
            {
                KeyboardState.HandledDownKeys.SetKeyUp(e.KeyCode);
            }

            handler(this, e);
        }

        private int _indentCounter = 0;

        protected override bool Callback(CallbackData data)
        {
            var eDownUp = GetDownUpEventArgs(data);

            _logger.LogInformation(new String('\t', _indentCounter++) + "→" + eDownUp.ToString());

            InvokeKeyDown(eDownUp);

            if (KeyPress != null)
            {
                var pressEventArgs = GetPressEventArgs(data);
                foreach (var pressEventArg in pressEventArgs)
                    InvokeKeyPress(pressEventArg);
            }

            InvokeKeyUp(eDownUp);
            _logger.LogInformation(new String('\t', --_indentCounter) + "←" + eDownUp.ToString());
            //Console.Write(Environment.NewLine);

            return !eDownUp.Handled;
        }

        protected abstract IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data);
        protected abstract IKeyEventArgs GetDownUpEventArgs(CallbackData data);
    }
}
