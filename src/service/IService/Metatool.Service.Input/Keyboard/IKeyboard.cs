﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Command;

namespace Metatool.Service
{
    public interface IKeyboard : IKeyboardVirtual
    {
        IKeyboardCommandTrigger Down(IHotkey sequenceUnit, string stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger Up(IHotkey sequenceUnit, string stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger AllUp(IHotkey sequenceUnit, string stateTree = KeyStateTrees.Default);

        IKeyboardCommandTrigger Hit(ISequenceUnit sequenceUnit, string stateTree = KeyStateTrees.Default);
     
        IKeyCommand Map(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null, int repeat = 1);

        IKeyCommand Map(string source, string target, Predicate<IKeyEventArgs> predicate = null);

        IKeyCommand HardMap(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null);

        IKeyCommand MapOnHit(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null, bool allUp = true);

        Task<IKeyEventArgs> KeyDownAsync(bool handled = false, CancellationToken token = default);
        Task<IKeyEventArgs> KeyUpAsync(bool handled = false, CancellationToken token = default);

    }

    public interface IKeyboardVirtual
    {
        void Type(Key key);
        void Type(Key[] keys);
        void Type(string text);
        void Type(char character);
    }
}
