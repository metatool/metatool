using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Input.MouseKeyHook.WinApi;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

/// <summary>
///     Provides extended argument data for the <see cref='KeyListener.KeyDown' /> or
///     <see cref='KeyListener.KeyUp' /> event.
/// </summary>
public class KeyEventArgsExt : KeyEventArgs, IKeyEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="IKeyEventArgs" /> class.
	/// </summary>
	/// <param name="keyData"></param>
	public KeyEventArgsExt(Keys keyData)
		: base(keyData)
	{
	}

	internal KeyEventArgsExt(Keys keyData, int scanCode, int timestamp, bool isKeyDown, bool isKeyUp,
		bool isExtendedKey, KeyEventArgsExt lastKeyEvent, KeyboardState keyboardState)
		: this(keyData)
	{
		ScanCode                                  = scanCode;
		Timestamp                                 = timestamp;
		IsKeyDown                                 = isKeyDown;
		IsKeyUp                                   = isKeyUp;
		IsExtendedKey                             = isExtendedKey;
		LastKeyEvent                              = lastKeyEvent;
		LastKeyDownEvent                          = lastKeyEvent.LastKeyDownEvent;
		LastKeyDownEvent_NoneVirtual              = lastKeyEvent.LastKeyDownEvent_NoneVirtual;
		LastKeyEvent_NoneVirtual                  = lastKeyEvent.LastKeyEvent_NoneVirtual;
		lastKeyEvent.LastKeyDownEvent             = null;
		lastKeyEvent.LastKeyEvent                 = null;
		lastKeyEvent.LastKeyDownEvent_NoneVirtual = null;
		lastKeyEvent.LastKeyEvent_NoneVirtual     = null;

		KeyboardState = keyboardState;
	}

	static Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

	public IKeyPath PathToGo { get; internal set; }

	public void BeginInvoke(Action action)
	{
		_dispatcher.BeginInvoke(DispatcherPriority.Send, action);
	}

	public void BeginInvoke(Action<IKeyEventArgs> action)
	{
		_dispatcher.BeginInvoke(DispatcherPriority.Send, action, this);
	}

	public static async Task<T> InvokeAsync<T>(Func<T> action,
		DispatcherPriority priority = DispatcherPriority.Send)
	{
		var o = _dispatcher.BeginInvoke(priority, action);
		await o;
		return (T) (o.Result);
	}

	public static async Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Send)
	{
		await InvokeAsync<object>(() =>
		{
			action();
			return null;
		}, priority);
	}


	/// <summary>
	///     The hardware scan code.
	/// </summary>
	public int ScanCode { get; }

	private Key _key;
    public  Key Key => _key??=new Key(KeyValues);
    public KeyValues KeyValues => KeyData.ToKeyValues();

    public new KeyValues KeyCode => base.KeyCode.ToKeyValues();

    /// <summary>
	/// is it from the keyboard simulator?
	/// </summary>
	public bool IsVirtual { get; private set; }

	/// <summary>
	///     The system tick count of when the event occurred.
	/// </summary>
	public int Timestamp { get; }

	/// <summary>
	///     True if event signals key down..
	/// </summary>
	public bool IsKeyDown { get; }

	public IKeyEventArgs LastKeyDownEvent { get; private set; }
	public IKeyEventArgs LastKeyEvent     { get; private set; }

	public   IKeyEventArgs LastKeyDownEvent_NoneVirtual { get; private set; }
	public   IKeyEventArgs LastKeyEvent_NoneVirtual     { get; private set; }
	internal KeyListener   listener;
	internal bool?         HandleVirtualKeyBackup;

	public void DisableVirtualKeyHandlingInThisEvent()
	{
		HandleVirtualKeyBackup    = listener.HandleVirtualKey;
		listener.HandleVirtualKey = false;
	}

	/// <summary>
	///     True if event signals key up.
	/// </summary>
	public bool IsKeyUp { get; }

	/// <summary>
	///     True if event signals, that the key is an extended key
	/// </summary>
	public bool IsExtendedKey { get; }

	public KeyEventType KeyEventType { get; internal set; }

	public new bool Handled
	{
		get => base.Handled;
		set
		{
			if (IsKeyDown && value) MouseKeyHook.Implementation.KeyboardState.HandledDownKeys.SetKeyDown(KeyCode);

			base.Handled = value;
		}
	}

    public bool           NoFurtherProcess { get; set; }
	public IKeyboardState KeyboardState    { get; }

	public override string ToString()
	{
		var dt = DateTime.Now;
		dt = dt.AddMilliseconds(Timestamp - Environment.TickCount);
		var d = IsKeyUp ? "Up" : "Down";
		return
			$"{dt:hh:mm:ss.fff}  {KeyCode,-16}{d,-6}Handled:{Handled,-8} IsVirtual: {IsVirtual,-8} Scan:{ScanCode,-8} Extended:{IsExtendedKey}  With: {KeyboardState}";
	}

	private static KeyEventArgsExt _lastKeyEventGloable = new(Keys.None);
	private static KeyEventArgsExt _lastKeyEventApp     = new(Keys.None);

	internal static IKeyEventArgs FromRawDataApp(CallbackData data)
	{
		var wParam = data.WParam;
		var lParam = data.LParam;

		//http://msdn.microsoft.com/en-us/library/ms644984(v=VS.85).aspx

		const uint maskKeydown     = 0x40000000; // for bit 30
		const uint maskKeyup       = 0x80000000; // for bit 31
		const uint maskExtendedKey = 0x1000000;  // for bit 24

		var timestamp = Environment.TickCount;

		var flags = (uint) lParam.ToInt64();

		//bit 30 Specifies the previous key state. The value is 1 if the key is down before the message is sent; it is 0 if the key is up.
		var wasKeyDown = (flags & maskKeydown) > 0;
		//bit 31 Specifies the transition state. The value is 0 if the key is being pressed and 1 if it is being released.
		var isKeyReleased = (flags & maskKeyup) > 0;
		//bit 24 Specifies the extended key state. The value is 1 if the key is an extended key, otherwise the value is 0.
		var isExtendedKey = (flags & maskExtendedKey) > 0;


		var keyData = AppendModifierStates((Keys) wParam);
		var scanCode = (int) (((flags & 0x10000)  | (flags & 0x20000)  | (flags & 0x40000)  | (flags & 0x80000) |
		                       (flags & 0x100000) | (flags & 0x200000) | (flags & 0x400000) | (flags & 0x800000)) >>
		                      16);

		var isKeyDown = !isKeyReleased;
		var isKeyUp   = wasKeyDown && isKeyReleased;

		var r = new KeyEventArgsExt(keyData, scanCode, timestamp, isKeyDown, isKeyUp, isExtendedKey,
			_lastKeyEventApp,
			MouseKeyHook.Implementation.KeyboardState.Current());
		_lastKeyEventApp = r;
		if (isKeyDown) _lastKeyEventApp.LastKeyDownEvent = r;
		return r;
	}

	internal static IKeyEventArgs FromRawDataGlobal(CallbackData data)
	{
		var wParam = data.WParam;
		var lParam = data.LParam;
		var keyboardHookStruct =
			(KeyboardHookStruct) Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

		var keyData   = AppendModifierStates((Keys) keyboardHookStruct.VirtualKeyCode);
		var isVirtual = (keyboardHookStruct.ExtraInfo & 0x01) == 0x01;

		var keyCode   = (int) wParam;
		var isKeyDown = keyCode == Messages.WM_KEYDOWN || keyCode == Messages.WM_SYSKEYDOWN;
		var isKeyUp   = keyCode == Messages.WM_KEYUP   || keyCode == Messages.WM_SYSKEYUP;

		const uint maskExtendedKey = 0x1;
		var        isExtendedKey   = (keyboardHookStruct.Flags & maskExtendedKey) > 0;

		var r = new KeyEventArgsExt(keyData, keyboardHookStruct.ScanCode, keyboardHookStruct.Time, isKeyDown,
			isKeyUp, isExtendedKey, _lastKeyEventGloable, MouseKeyHook.Implementation.KeyboardState.Current());
		_lastKeyEventGloable = r;
		if (isKeyDown) _lastKeyEventGloable.LastKeyDownEvent                           = r;
		if (!isVirtual) _lastKeyEventGloable.LastKeyEvent_NoneVirtual                  = r;
		if (isKeyDown && !isVirtual) _lastKeyEventGloable.LastKeyDownEvent_NoneVirtual = r;

		r.IsVirtual = isVirtual;
		return r;
	}

	// # It is not possible to distinguish Keys.LControlKey and Keys.RControlKey when they are modifiers
	// Check for Keys.Control instead
	// Same for Shift and Alt(Menu)
	// See more at http://www.tech-archive.net/Archive/DotNet/microsoft.public.dotnet.framework.windowsforms/2008-04/msg00127.html #

	// A shortcut to make life easier
	private static bool CheckModifier(int vKey)
	{
		return (KeyboardNativeMethods.GetKeyState(vKey) & 0x8000) > 0;
	}

	private static Keys AppendModifierStates(Keys keyData)
	{
		// Is Control being held down?
		var control = CheckModifier(KeyboardNativeMethods.VK_CONTROL);
		// Is Shift being held down?
		var shift = CheckModifier(KeyboardNativeMethods.VK_SHIFT);
		// Is Alt being held down?
		var alt = CheckModifier(KeyboardNativeMethods.VK_MENU);

		// Windows keys
		// # combine LWin and RWin key with other keys will potentially corrupt the data
		// notable F5 | Keys.LWin == F12, see https://globalmousekeyhook.codeplex.com/workitem/1188
		// and the KeyEventArgs.KeyData don't recognize combined data either

		// Function (Fn) key
		// # CANNOT determine state due to conversion inside keyboard
		// See http://en.wikipedia.org/wiki/Fn_key#Technical_details #

		return keyData                              |
		       (control ? Keys.Control : Keys.None) |
		       (shift ? Keys.Shift : Keys.None)     |
		       (alt ? Keys.Alt : Keys.None);
	}
}