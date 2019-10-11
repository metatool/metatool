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
using Metatool.MetaKeyboard;
using Metatool.Plugin;
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
            _default ??= ServiceLocator.GetService<IKeyboard, Keyboard>();

        public Keyboard(ILogger<Keyboard> logger)
        {
            _logger = logger;
            Hook();
        }

        public IKeyPath Root = null;

        readonly KeyboardHook _hook =
            new KeyboardHook(ServiceLocator.GetService<ILogger<KeyboardHook>>());

        readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        internal IMetaKey Add(ICombination combination, KeyEvent keyEvent, KeyCommand command,
            KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            return Add(new List<ICombination> {combination}, keyEvent, command, stateTree);
        }


        internal IMetaKey Add(IList<ICombination> combinations, KeyEvent keyEvent, KeyCommand command,
            KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            foreach (var combination in combinations)
            {
                foreach (var key in combination.Chord)
                {
                    key.Then()
                }
            }
            return _hook.Add(combinations, new KeyEventCommand(keyEvent, command), stateTree);
        }

        public void ShowTip()
        {
            _hook.ShowTip();
        }

        /// <summary>
        /// down up happened successively
        /// </summary>
        internal IKeyboardCommandToken Hit(ICombination combination, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            var           handling     = false;
            IKeyEventArgs keyDownEvent = null;
            var token = new KeyboardCommandTokens
            {
                combination.Down(e =>
                {
                    handling     = true;
                    keyDownEvent = e;
                }, canExecute, description,stateTree),

                combination.Up(e =>
                {
                    if (!handling)
                    {
                        Console.WriteLine($"\t{combination}_Hit Down CanExecute:false");
                        return;
                    }

                    handling = false;

                    if (keyDownEvent == e.LastKeyDownEvent)
                    {
                        e.BeginInvoke(() => execute(e));
                    }
                    else
                    {
                        Console.WriteLine($"\t{combination}_Hit: last down event is not from me, Not Execute!");
                    }
                }, canExecute, description,stateTree)
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

        public event KeyEventHandler KeyDown
        {
            add => _hook.KeyDown += value;
            remove => _hook.KeyDown -= value;
        }

        public event KeyEventHandler KeyUp
        {
            add => _hook.KeyUp += value;
            remove => _hook.KeyUp -= value;
        }

        internal void HotKey(string keys, string description, Action action)
        {
            if (keys.Contains(','))
            {
                var sequence = Sequence.FromString(keys).ToList();
                Add(sequence, KeyEvent.Down, new KeyCommand(e => action()) {Description = description});
            }

            var combination = Combination.FromString(keys) as Combination;
            Add(combination, KeyEvent.Down, new KeyCommand(e => action()) {Description = description});
        }

        private void Async(Action action, DispatcherPriority priority = DispatcherPriority.Send)
        {
            _dispatcher.BeginInvoke(priority, action);
        }



        private void Hook()
        {
            _logger.LogInformation("Keyboard hook is running...");
            _hook.Run();
        }

       
    }
}