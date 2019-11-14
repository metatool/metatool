﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Metatool.Command;
using Metatool.Input.MouseKeyHook;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.Service.MouseKeyHook;
using Metatool.Service.MouseKeyHook.Implementation;
using Metatool.UI;
using Metatool.WindowsInput.Native;
using Microsoft.Extensions.Logging;
using KeyEventHandler = Metatool.Input.MouseKeyHook.KeyEventHandler;

namespace Metatool.Input
{
    public partial class Keyboard : IKeyboard, IKeyboardInternal
    {
        private readonly ILogger<Keyboard> _logger;
        private static   Keyboard          _default;

        public static Keyboard Default =>
            _default ??= Services.Get<IKeyboard, Keyboard>();

        public Keyboard(ILogger<Keyboard> logger)
        {
            _logger = logger;
            Hook();
        }

        public IKeyPath Root = null;

        readonly KeyboardHook _hook =
            new KeyboardHook(Services.Get<ILogger<KeyboardHook>>(), Services.Get<INotify>());

        readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        internal IMetaKey Add(IList<ICombination> sequence, KeyEvent keyEvent, KeyCommand command,
            string stateTree = KeyStateTrees.Default)
        {
            foreach (var combination in sequence)
            {
                foreach (var keyInChord in combination.Chord)
                {
                    foreach (var code in keyInChord.Codes)
                    {
                        var key = (Key) code;
                        if (!key.IsCommonChordKey())
                        {
                            var keyStateTree = KeyStateTree.GetOrCreateStateTree(KeyStateTrees.ChordMap);
                            if (!keyStateTree.Contains(key))
                                MapOnHit(key.ToCombination(), key.ToCombination(), e => !e.IsVirtual, false);
                        }
                    }
                }
            }

            return _hook.Add(sequence, new KeyEventCommand(keyEvent, command), stateTree);
        }

        public void ShowTip()
        {
            _hook.ShowTip();
        }

        /// <summary>
        /// down up happened successively
        /// </summary>
        internal IKeyCommand Hit(IHotkey hotkey, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "",
            string stateTree = KeyStateTrees.Default)
        {
            var           handling     = false;
            IKeyEventArgs keyDownEvent = null;
            var token = new KeyCommandTokens
            {
                hotkey.Down(e =>
                {
                    handling     = true;
                    keyDownEvent = e;
                }, canExecute, description, stateTree),

                hotkey.Up(e =>
                {
                    if (!handling)
                    {
                        Console.WriteLine($"\t{hotkey}_Hit Down CanExecute:false");
                        return;
                    }

                    handling = false;

                    if (keyDownEvent == e.LastKeyDownEvent)
                    {
                        e.BeginInvoke(() => execute(e));
                    }
                    else
                    {
                        Console.WriteLine($"\t{hotkey}_Hit: last down event is not from me, Not Execute!");
                    }
                }, canExecute, description, stateTree)
            };

            return token;
        }

        public void Hit(Keys key, IEnumerable<Keys> modifierKeys = null, bool isAsync = false)
        {
            if (isAsync)
            {
                Async(() => Hit(key, modifierKeys));
                return;
            }

            if (modifierKeys == null) InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) key);
            InputSimu.Inst.Keyboard.ModifiedKeyStroke(modifierKeys.Cast<VirtualKeyCode>(),
                (VirtualKeyCode) key);
        }

        private Action Repeat(int repeat, Action action)
        {
            return () =>
            {
                while (repeat-- > 0) action();
            };
        }

        public event KeyPressEventHandler KeyPress
        {
            add => _hook.KeyPress += value;
            remove => _hook.KeyPress -= value;
        }

        public event MouseKeyHook.KeyEventHandler KeyDown
        {
            add => _hook.KeyDown += value;
            remove => _hook.KeyDown -= value;
        }

        public event MouseKeyHook.KeyEventHandler KeyUp
        {
            add => _hook.KeyUp += value;
            remove => _hook.KeyUp -= value;
        }

        private void Async(Action action, DispatcherPriority priority = DispatcherPriority.Send)
        {
            _dispatcher.BeginInvoke(priority, action);
        }

        private void Hook()
        {
            _hook.Run();
        }
    }
}