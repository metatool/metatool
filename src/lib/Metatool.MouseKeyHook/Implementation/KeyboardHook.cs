using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Metaseed;

namespace Metatool.Input.MouseKeyHook;

public delegate void KeyEventHandler(object sender, IKeyEventArgs e);

public delegate void KeyPressEventHandler(object sender, IKeyPressEventArgs e);
public record KeyTipNotifier(Action<string, IEnumerable<(string key, IEnumerable<string> descriptions)>> showKeysTip, Action<string> closeKeysTip) : IKeyTipNotifier
{
    public void ShowKeysTip(string name, IEnumerable<(string key, IEnumerable<string> descriptions)> tips) => showKeysTip(name, tips);
    public void CloseKeysTip(string name) => closeKeysTip(name);
}

public class KeyboardHook
{
    private readonly ILogger<KeyboardHook> _logger;
    private readonly IKeyboardMouseEvents _eventSource;
    private bool _isRunning;
    private IFruitMonkey _monkey;

    public bool Disable
    {
        get => _eventSource.Disable;
        set => _eventSource.Disable = value;
    }
    public bool DisableDownEvent
    {
        get => _eventSource.DisableDownEvent;
        set => _eventSource.DisableDownEvent = value;
    }
    public bool DisableUpEvent
    {
        get => _eventSource.DisableUpEvent;
        set => _eventSource.DisableUpEvent = value;
    }
    public bool DisablePressEvent
    {
        get => _eventSource.DisablePressEvent;
        set => _eventSource.DisablePressEvent = value;
    }

    public KeyboardHook(ILogger<KeyboardHook> logger, INotify notify)
    {
        _logger = logger;
        _eventSource = Hook.GlobalEvents();
        _monkey = new FruitMonkey(logger, new KeyTipNotifier((string key, IEnumerable<(string key, IEnumerable<string> descriptions)> tips) => notify.ShowKeysTip(key, tips), key => notify.CloseKeysTip(key)));
#if DEBUG
        DebugState.Watcher.Add("Forest", _monkey.Forest);
#endif
    }

    private readonly List<KeyEventHandler> _keyUpHandlers = [];

    public event KeyEventHandler KeyUp
    {
        add => _keyUpHandlers.Add(value);
        remove => _keyUpHandlers.Remove(value);
    }

    private readonly List<KeyPressEventHandler> _keyPressHandlers = [];

    public event KeyPressEventHandler KeyPress
    {
        add => _keyPressHandlers.Add(value);
        remove => _keyPressHandlers.Remove(value);
    }

    public bool HandleVirtualKey
    {
        get => _eventSource.HandleVirtualKey;
        set => _eventSource.HandleVirtualKey = value;
    }

    private readonly List<KeyEventHandler> _keyDownHandlers = [];

    public event KeyEventHandler KeyDown
    {
        add => _keyDownHandlers.Add(value);
        remove => _keyDownHandlers.Remove(value);
    }

    public IMetaKey Add(IList<ICombination> combinations, KeyEventCommand command, string stateTree = KeyStateTrees.Default) => _monkey.Forest.Add(combinations, command, stateTree);
    public void ShowTip(bool ifRootThenEmpty = false) => _monkey.Forest.ShowTip(ifRootThenEmpty);
    public void DisableChord(Chord chord, string stateTree = null) => _monkey.Forest.DisableChord(chord, stateTree);
    public void EnableChord(Chord chord, string stateTree = null) => _monkey.Forest.EnableChord(chord, stateTree);
    public bool Contains(IHotkey hotKey, string stateTree = null) => _monkey.Forest.Contains(hotKey, stateTree);

    public void Run()
    {
        if (_isRunning) return;
        Debug.Assert(System.Windows.Application.Current.Dispatcher != null,
            "System.Windows.Application.Current.Dispatcher != null");
        var access = System.Windows.Application.Current.Dispatcher.CheckAccess();
        if (!access)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)Run);
            return;
        }

        _isRunning = true;
        _logger.LogInformation($"Keyboard hook is running...");

        _monkey.Reset();

        _eventSource.KeyDown += (sender, args) =>
        {
            _monkey.ClimbTree(KeyEventType.Down, args);
            // a copy, so newly added would be handled in next event.
            List<KeyEventHandler> handlers = [.. _keyDownHandlers];
            handlers.ForEach(h => h?.Invoke(sender, args));
        };

        _eventSource.KeyPress += (sender, args) =>
        {
            List<KeyPressEventHandler> handlers = [.. _keyPressHandlers]; // a copy
            handlers.ForEach(h => h?.Invoke(sender, args));
        };

        _eventSource.KeyUp += (sender, args) =>
        {
            _monkey.ClimbTree(KeyEventType.Up, args);
            List<KeyEventHandler> handlers = [.. _keyUpHandlers]; // a copy
            handlers.ForEach(h => h?.Invoke(sender, args));
        };
    }
}