﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Command;

namespace Metatool.Service;

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
	/// <summary>
	/// i.e. Z: LCtrl+LShift, then press Z+A = LCtrl+LShift+A
	/// if Z pressed and then release, z would be typed
	/// if Z is long time pressed(2s by default), it would be repeated
	/// </summary>
	/// <param name="source"></param>
	/// <param name="target"></param>
	/// <param name="predicate"></param>
	/// <returns></returns>
	IKeyCommand ChordMap(ISequenceUnit source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate = null);
	IKeyCommand Map(IHotkey source, IHotkey target, KeyMaps keyMaps, Predicate<IKeyEventArgs> predicate = null);

	IKeyCommand HotString(string source, string target, Predicate<IKeyEventArgs> predicate = null);
	void AddHotStrings(IDictionary<string, HotStringDef> hotStrings);

	Task<IKeyEventArgs> KeyDownAsync(bool handled = false, int timeout = Timeout.Infinite, ISequenceUnit hotkey = null, CancellationToken token = default);
	Task<IKeyPressEventArgs> KeyPressAsync(bool handled = false, int timeout = Timeout.Infinite, ISequenceUnit hotkey = null, CancellationToken token = default);
	Task<IKeyEventArgs> KeyUpAsync(bool handled = false, int timeout = Timeout.Infinite, ISequenceUnit hotkey = null, CancellationToken token = default);

	bool AddAliases(IDictionary<string, string> aliases);
	Dictionary<string, IHotkey> Aliases { get; }
	bool RegisterKeyMaps(IDictionary<string, KeyMapDef> maps, IDictionary<string, string> additionalAliases = null);
	string ReplaceAlias(string hotkey, params IDictionary<string, string>[] additionalTempAliasesDics);

	bool IsDown(IKey key);
	bool IsUp(IKey key);
	bool IsToggled(IKey key);

	bool Disable { get; set; }
	bool DisableDownEvent { get; set; }
	bool DisableUpEvent { get; set; }
	bool DisablePressEvent { get; set; }

	bool HandleVirtualKey { get; set; }
	IKeyboardState State { get; }
	void DisableChord(ISequenceUnit chord);
	void EnableChord(ISequenceUnit chord);

}

public enum KeyMaps
{
	/// <summary>
	/// map to target down when source down, map to target up when source up
	/// </summary>
	MapOnDownUp,
	/// <summary>
	/// replay the broken key, i.e. Num0 for RCtrl
	/// </summary>
	HardMap,
	/// <summary>
	/// do mapping when the triggerKey is downAndUp
	/// Note: A+B -> C become A+C, use MapOnHitAndAllUp to overcome
	/// </summary>
	MapOnHit,
	MapOnHitAndAllUp,
	/// <summary>
	/// i.e. x+ for ctrl+alt+
	/// </summary>
	ChordMap
}

public interface IKeyboardVirtual
{
	void Type(params IHotkey[] keys);
	void Down(params IHotkey[] keys);
	void Up(params IHotkey[] keys);
	void Type(string text);
	void Type(char character);
}