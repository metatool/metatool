using Metatool.Input.MouseKeyHook;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public partial class Keyboard
{
	private readonly ILogger<Keyboard>       _logger;
	private readonly IConfig<MetatoolConfig> _config;
	private static   Keyboard                _default;

	public static Keyboard Default =>
		_default ??= Services.Get<IKeyboard, Keyboard>();

	public Keyboard(ILogger<Keyboard> logger, IConfig<MetatoolConfig> config, IUiDispatcher dispatcher)
	{
		_logger = logger;
		_config = config;
		var aliases = config.CurrentValue.Services.Input.Keyboard.KeyAliases;
		AddAliases(aliases);
		Hook();
        // workaround to use the Keyboard Service itself via DI in initService
        Task.Run(() => InitService(config));
	}

	private void InitService(IConfig<MetatoolConfig> config)
	{
		var keyboard   = config.CurrentValue.Services.Input.Keyboard;
		var hotStrings = keyboard.HotStrings;
		AddHotStrings(hotStrings);

		keyboard.Hotkeys.TryGetValue("Reset", out var resetTrigger);
		resetTrigger?.OnEvent(_ => ReleaseDownKeys());
	}

	public void AddHotStrings(IDictionary<string, HotStringDef> hotStrings)
	{
		foreach (var (key, strings) in hotStrings)
		{
			foreach (var str in strings)
			{
				HotString(key, str);
			}
		}
	}

	public IKeyPath Root = null;

	readonly KeyboardHook _hook = new(Services.Get<ILogger<KeyboardHook>>(), Services.Get<INotify>());

	internal IMetaKey Add(IList<ICombination> sequence, KeyEventType keyEventType, KeyCommand command,
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
						if(!_hook.Contains(key, KeyStateTrees.ChordMap))
							MapOnHit(key.ToCombination(), key.ToCombination(), e => !e.IsVirtual, "MapOnHit on ChordMapTree");
					}
				}
			}
		}

		return _hook.Add(sequence, new KeyEventCommand(keyEventType, command), stateTree);
	}

	public void ShowTip()
	{
		_hook.ShowTip();
	}

	/// <summary>
	/// down up happened successively
	/// </summary>
	private IKeyCommand Hit(IHotkey hotkey, Action<IKeyEventArgs> execute,
		Predicate<IKeyEventArgs> canExecute = null, string description = "",
		string stateTree = KeyStateTrees.Default)
	{
		var noEventTimer = new NoEventTimer();
		// state
		var           handling     = false;
		IKeyEventArgs keyDownEvent = null;

		void Reset()
		{
			handling     = false;
			keyDownEvent = null;
		}

		var token = new KeyCommandTokens
		{
			hotkey.OnDown(e =>
			{
				handling     = true;
				keyDownEvent = e;
			}, e =>
			{
				var noEventDuration = noEventTimer.NoEventDuration;
				if (noEventDuration > StateResetTime) Reset();
				noEventTimer.EventPulse();

				return canExecute?.Invoke(e) ?? true;
			}, description, stateTree),

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
					execute(e);
				}
				else
				{
					Console.WriteLine($"\t{hotkey}_Hit: last down event is not from me, Not Execute!");
				}
			}, canExecute, description, stateTree)
		};

		return token;
	}

	public bool HandleVirtualKey
	{
		get => _hook.HandleVirtualKey;
		set => _hook.HandleVirtualKey = value;
	}


	public event MouseKeyHook.KeyPressEventHandler KeyPress
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

    private Thread _hookThread;
    private void Hook()
	{
        _hookThread = new Thread(_hook.Run)
        {
            Name = "Keyboard/Mouse Hook Thread",
            IsBackground = true,
            Priority = ThreadPriority.Highest
        };
        _hookThread.Start();
    }
}