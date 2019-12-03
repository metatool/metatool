using System;
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
using Metatool.Service.Internal;
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
        private readonly IConfig<MetatoolConfig> _config;
        private static   Keyboard          _default;

        public static Keyboard Default =>
            _default ??= Services.Get<IKeyboard, Keyboard>();

        public Keyboard(ILogger<Keyboard> logger, IConfig<MetatoolConfig> config)
        {
            _logger = logger;
            _config = config;
            var aliases = config.CurrentValue.Services.Input.Keyboard.KeyAliases;
            AddAliases(aliases);
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
                                MapOnHit(key.ToCombination(), key.ToCombination(), e => !e.IsVirtual);
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
                hotkey.OnDown(e =>
                {
                    handling     = true;
                    keyDownEvent = e;
                }, canExecute, description, stateTree),

                hotkey.OnUp(e =>
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

        private void Hook()
        {
            _hook.Run();
        }
    }
}