using System;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Command;

namespace Metatool.Service
{
    public interface IKeyboard : IKeyboardVirtual
    {
        IKeyboardCommandTrigger Down(IHotkey hotkey, string stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger Up(IHotkey hotkey, string stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger AllUp(IHotkey hotkey, string stateTree = KeyStateTrees.Default);

        IKeyboardCommandTrigger Hit(IHotkey hotkey, string stateTree = KeyStateTrees.Default);
        IKeyCommand Map(IHotkey source, ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null, int repeat = 1);
        IKeyCommand HardMap(IHotkey source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null);
        IKeyCommand MapOnHit(IHotkey source, ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null, bool allUp = true);

        IKeyCommand HotString(string source, string target, Predicate<IKeyEventArgs> predicate = null);

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
