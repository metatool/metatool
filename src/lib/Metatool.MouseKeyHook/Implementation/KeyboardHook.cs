using Metaseed;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

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
    private const uint WM_USER = 0x0400;
    private const uint WM_EXECUTE_ACTION = WM_USER + 1;

    private readonly ILogger<KeyboardHook> _logger;
    private readonly IKeyboardMouseEvents _eventSource;
    private bool _isRunning;
    private readonly IFruitMonkey _monkey;
    private readonly ConcurrentQueue<Action> _actionQueue = new();
    private uint _threadId;

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

    private WebViewHost.WebViewHost _webUI;

    public KeyboardHook(ILogger<KeyboardHook> logger, INotify notify)
    {
        _logger = logger;
        _eventSource = Hook.GlobalEvents();
        Application.Current.Dispatcher.BeginInvoke(() => _webUI = new WebViewHost.WebViewHost());
        var notifier = new KeyTipNotifier((key, tips) =>
        {
            _webUI?.ShowSearch(tips, item =>
            {
                _logger.LogInformation($"selected key: {item.hotkey}, description: {item.description}");
                
            });
        }, key => { });
        // var  notifier = new KeyTipNotifier((key, tips) => notify.ShowKeysTip(key, tips), key => notify.CloseKeysTip(key));
        _monkey = new FruitMonkey(logger, notifier);

        DebugState.Add("Forest", _monkey.Forest);
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

    /// <summary>
    /// Posts an action to be executed on the message loop thread.
    /// </summary>
    public void Post(Action action)
    {
        if (action == null) return;
        _actionQueue.Enqueue(action);
        PostThreadMessage(_threadId, WM_EXECUTE_ACTION, IntPtr.Zero, IntPtr.Zero);
    }

    /// <summary>
    /// Sends an action to be executed on the message loop thread and waits for completion.
    /// </summary>
    public void Send(Action action)
    {
        if (action == null) return;

        if (GetCurrentThreadId() == _threadId)
        {
            action();
            return;
        }

        using var completionEvent = new ManualResetEventSlim(false);
        Exception capturedException = null;

        _actionQueue.Enqueue(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                capturedException = ex;
            }
            finally
            {
                completionEvent.Set();
            }
        });

        PostThreadMessage(_threadId, WM_EXECUTE_ACTION, IntPtr.Zero, IntPtr.Zero);
        completionEvent.Wait();

        if (capturedException != null)
            throw new AggregateException("send Action execution failed on KeyboardHook message loop thread:{Thread.CurrentThread.Name}", capturedException);
    }

    public void Run()
    {
        if (_isRunning) return;

        _isRunning = true;
        _logger.LogInformation($"Keyboard hook is running...");

        _monkey.Reset();

        _eventSource.KeyDown += (sender, args) =>
        {
            _monkey.ClimbTree(args);
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
            _monkey.ClimbTree(args);
            List<KeyEventHandler> handlers = [.. _keyUpHandlers]; // a copy
            handlers.ForEach(h => h?.Invoke(sender, args));
        };

        _threadId = GetCurrentThreadId();

        while (GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
        {
            if (msg.message == WM_EXECUTE_ACTION)
            {
                ProcessActionQueue();
                continue;
            }

            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }

    private void ProcessActionQueue()
    {
        while (_actionQueue.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing action on message KeyboardHook thread: {Thread.CurrentThread.Name}");
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

    [DllImport("user32.dll")]
    private static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();
}