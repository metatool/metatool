using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Command;

namespace Metatool.Service
{
    public interface IKeyboard : IKeyboardVirtual
    {
        IKeyboardCommandTrigger OnDown(IHotkey hotkey, string stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger OnUp(IHotkey hotkey, string stateTree = KeyStateTrees.Default);
        /// down up happened successively
        IKeyboardCommandTrigger OnHit(IHotkey hotkey, string stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger OnAllUp(IHotkey hotkey, string stateTree = KeyStateTrees.Default);
        IKeyboardCommandTrigger OnEvent(IHotkey hotkey, KeyEvent keyEvent, string stateTree = KeyStateTrees.Default);

        IKeyCommand HardMap(IHotkey source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate = null);
        IKeyCommand MapOnDownUp(IHotkey source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate = null);
        IKeyCommand MapOnHit(IHotkey source, IHotkey target, Predicate<IKeyEventArgs> predicate = null);
        IKeyCommand MapOnHitAndAllUp(IHotkey source, IHotkey target, Predicate<IKeyEventArgs> predicate = null);
        IKeyCommand Map(IHotkey source, IHotkey target, KeyMaps keyMaps, Predicate<IKeyEventArgs> predicate = null);

        IKeyCommand ChordMap(ISequenceUnit source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate = null);

        IKeyCommand HotString(string source, string target, Predicate<IKeyEventArgs> predicate = null);

        Task<IKeyEventArgs> KeyDownAsync(bool handled = false, CancellationToken token = default);
        Task<IKeyEventArgs> KeyUpAsync(bool handled = false, CancellationToken token = default);

        bool AddAliases(IDictionary<string, string> aliases);
        Dictionary<string, IHotkey> Aliases { get; }
        bool RegisterKeyMaps(IDictionary<string, string> maps, IDictionary<string, string> additionalAliases = null);
        string ReplaceAlias(string hotkey, params IDictionary<string, string>[] additionalAliasesDics);

    }

    public enum KeyMaps { HardMap, MapOnDownUp, MapOnHit, MapOnHitAndAllUp}

    public interface IKeyboardVirtual
    {
        void Type(IHotkey hotkey);
        void Down(IHotkey hotkey);
        void Up(IHotkey hotkey);
        void Type(string text);
        void Type(char character);
    }
}