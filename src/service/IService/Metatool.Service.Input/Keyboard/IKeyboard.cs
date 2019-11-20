using System;
using System.Collections.Generic;
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
        IKeyboardCommandTrigger Event(IHotkey hotkey, KeyEvent keyEvent, string stateTree = KeyStateTrees.Default);

        IKeyCommand Map(IHotkey source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate = null, int repeat = 1);
        IKeyCommand HardMap(IHotkey source, ICombination target, Predicate<IKeyEventArgs> predicate = null);
        IKeyCommand MapOnHit(IHotkey source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate = null, bool allUp = true);

        IKeyCommand HotString(string source, string target, Predicate<IKeyEventArgs> predicate = null);

        Task<IKeyEventArgs> KeyDownAsync(bool handled = false, CancellationToken token = default);
        Task<IKeyEventArgs> KeyUpAsync(bool handled = false, CancellationToken token = default);
        bool AddAliases(IDictionary<string, string> aliases);
        Dictionary<string, IHotkey> Aliases { get; }
        bool RegisterKeyMaps(IDictionary<string, string> maps, IDictionary<string, string> additionalAliases=null);
        string ReplaceAlias(string hotkey, params IDictionary<string, string>[] additionalAliasesDics);
    }

    public interface IKeyboardVirtual
    {
        void Type(Key key);
        void Type(Key[] keys);
        void Type(string text);
        void Type(char character);
    }
}
