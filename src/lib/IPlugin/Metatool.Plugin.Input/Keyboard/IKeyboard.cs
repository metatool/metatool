using System;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Command;

namespace Metatool.Input
{
    public interface IKeyboardVirtual
    {
        void Type(Key key);
        void Type(Key[] keys);
        void Type(string text);
        void Type(char character);
    }
    public interface IKeyboard : IKeyboardVirtual
    {
        IKeyboardCommandTrigger Down(ISequenceUnit sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger Up(ISequenceUnit sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger AllUp(ISequenceUnit sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger Hit(ISequenceUnit sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default);

        IKeyboardCommandTrigger Down(ISequence sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger Up(ISequence sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger AllUp(ISequence sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default);

        IKeyboardCommandToken Map(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null, int repeat = 1);

        IKeyboardCommandToken Map(string source, string target, Predicate<IKeyEventArgs> predicate = null);

        IKeyboardCommandToken HardMap(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null);

        IKeyboardCommandToken MapOnHit(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null, bool allUp = true);

        Task<IKeyEventArgs> KeyDownAsync(bool handled = false, CancellationToken token = default);
        Task<IKeyEventArgs> KeyUpAsync(bool handled = false, CancellationToken token = default);

    }
}