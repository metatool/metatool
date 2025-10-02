using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metatool.Service.MouseKey;

public partial class Key
{
	// const                  Keys AnyKeyCode    = Keys.KeyCode;
	public static readonly Key None = new(KeyCodes.None);

	// public static readonly Key  Any           = new Key(AnyKeyCode);
	public static readonly Key Caps          = new(KeyCodes.CapsLock);
	public static readonly Key CapsLock      = Caps;
	public static readonly Key LCtrl         = new(KeyCodes.LControlKey);
	public static readonly Key RCtrl         = new(KeyCodes.RControlKey);
	public static readonly Key CtrlKey       = new(KeyCodes.ControlKey);
	public static readonly Key Ctrl          = new(KeyCodes.LControlKey, KeyCodes.RControlKey);
	public static readonly Key CtrlChord     = new(KeyCodes.Control);
	public static readonly Key LShift        = new(KeyCodes.LShiftKey);
	public static readonly Key RShift        = new(KeyCodes.RShiftKey);
	public static readonly Key ShiftKey      = new(KeyCodes.ShiftKey);
	public static readonly Key Shift         = new(KeyCodes.LShiftKey, KeyCodes.RShiftKey);
	public static readonly Key ShiftChord    = new(KeyCodes.Shift);
	public static readonly Key LAlt          = new(KeyCodes.LMenu);
	public static readonly Key LMenu         = LAlt;
	public static readonly Key RAlt          = new(KeyCodes.RMenu);
	public static readonly Key RMenu         = RAlt;
	public static readonly Key AltKey        = new(KeyCodes.Menu);
	public static readonly Key Alt           = new(KeyCodes.LMenu, KeyCodes.RMenu);
	public static readonly Key Menu          = AltKey;
	public static readonly Key AltChord      = new(KeyCodes.Alt);
	public static readonly Key Enter         = new(KeyCodes.Enter);
	public static readonly Key Return        = Enter;
	public static readonly Key Tab           = new(KeyCodes.Tab);
	public static readonly Key LWin          = new(KeyCodes.LWin);
	public static readonly Key RWin          = new(KeyCodes.RWin);
	public static readonly Key Win           = new(KeyCodes.LWin, KeyCodes.RWin);
	public static readonly Key Apps          = new(KeyCodes.Apps);
	public static readonly Key Back          = new(KeyCodes.Back);
	public static readonly Key Backspace     = Back;
	public static readonly Key BS            = Back;
	public static readonly Key Space         = new(KeyCodes.Space);
	public static readonly Key SpaceBar      = Space;
	public static readonly Key A             = new(KeyCodes.A);
	public static readonly Key B             = new(KeyCodes.B);
	public static readonly Key C             = new(KeyCodes.C);
	public static readonly Key D             = new(KeyCodes.D);
	public static readonly Key E             = new(KeyCodes.E);
	public static readonly Key F             = new(KeyCodes.F);
	public static readonly Key G             = new(KeyCodes.G);
	public static readonly Key H             = new(KeyCodes.H);
	public static readonly Key I             = new(KeyCodes.I);
	public static readonly Key J             = new(KeyCodes.J);
	public static readonly Key K             = new(KeyCodes.K);
	public static readonly Key L             = new(KeyCodes.L);
	public static readonly Key M             = new(KeyCodes.M);
	public static readonly Key N             = new(KeyCodes.N);
	public static readonly Key O             = new(KeyCodes.O);
	public static readonly Key P             = new(KeyCodes.P);
	public static readonly Key Q             = new(KeyCodes.Q);
	public static readonly Key R             = new(KeyCodes.R);
	public static readonly Key S             = new(KeyCodes.S);
	public static readonly Key T             = new(KeyCodes.T);
	public static readonly Key U             = new(KeyCodes.U);
	public static readonly Key V             = new(KeyCodes.V);
	public static readonly Key W             = new(KeyCodes.W);
	public static readonly Key X             = new(KeyCodes.X);
	public static readonly Key Y             = new(KeyCodes.Y);
	public static readonly Key Z             = new(KeyCodes.Z);
	public static readonly Key Period        = new(KeyCodes.OemPeriod);
	public static readonly Key Comma         = new(KeyCodes.Oemcomma);
	public static readonly Key Question      = new(KeyCodes.OemQuestion);
	public static readonly Key Slash         = Question;
	public static readonly Key Quotes        = new(KeyCodes.OemQuotes);
	public static readonly Key SemiColon     = new(KeyCodes.OemSemicolon);
	public static readonly Key OpenBrackets  = new(KeyCodes.OemOpenBrackets);
	public static readonly Key CloseBrackets = new(KeyCodes.OemCloseBrackets);

	/// <summary>
	/// On US keyboard please use Pipe
	/// </summary>
	public static readonly Key Backslash = new(KeyCodes.OemBackslash);

	/// <summary>
	/// Blackslash and Pipe on US keyboard
	/// </summary>
	public static readonly Key Pipe = new(KeyCodes.OemPipe);

	public static readonly Key Esc         = new(KeyCodes.Escape);
	public static readonly Key Tilde       = new(KeyCodes.Oemtilde);
	public static readonly Key D1          = new(KeyCodes.D1);
	public static readonly Key D2          = new(KeyCodes.D2);
	public static readonly Key D3          = new(KeyCodes.D3);
	public static readonly Key D4          = new(KeyCodes.D4);
	public static readonly Key D5          = new(KeyCodes.D5);
	public static readonly Key D6          = new(KeyCodes.D6);
	public static readonly Key D7          = new(KeyCodes.D7);
	public static readonly Key D8          = new(KeyCodes.D8);
	public static readonly Key D9          = new(KeyCodes.D9);
	public static readonly Key D0          = new(KeyCodes.D0);
	public static readonly Key Minus       = new(KeyCodes.OemMinus);
	public static readonly Key Plus        = new(KeyCodes.Oemplus);
	public static readonly Key F1          = new(KeyCodes.F1);
	public static readonly Key F2          = new(KeyCodes.F2);
	public static readonly Key F3          = new(KeyCodes.F3);
	public static readonly Key F4          = new(KeyCodes.F4);
	public static readonly Key F5          = new(KeyCodes.F5);
	public static readonly Key F6          = new(KeyCodes.F6);
	public static readonly Key F7          = new(KeyCodes.F7);
	public static readonly Key F8          = new(KeyCodes.F8);
	public static readonly Key F9          = new(KeyCodes.F9);
	public static readonly Key F10         = new(KeyCodes.F10);
	public static readonly Key F11         = new(KeyCodes.F11);
	public static readonly Key F12         = new(KeyCodes.F12);
	public static readonly Key Ins         = new(KeyCodes.Insert);
	public static readonly Key Insert      = Ins;
	public static readonly Key Del         = new(KeyCodes.Delete);
	public static readonly Key Home        = new(KeyCodes.Home);
	public static readonly Key End         = new(KeyCodes.End);
	public static readonly Key Up          = new(KeyCodes.Up);
	public static readonly Key Down        = new(KeyCodes.Down);
	public static readonly Key Left        = new(KeyCodes.Left);
	public static readonly Key Right       = new(KeyCodes.Right);
	public static readonly Key PageUp      = new(KeyCodes.PageUp);
	public static readonly Key Prior       = PageUp;
	public static readonly Key PageDown    = new(KeyCodes.PageDown);
	public static readonly Key Next        = PageDown;
	public static readonly Key PrintScreen = new(KeyCodes.PrintScreen);
	public static readonly Key Snapshot    = PrintScreen;
	public static readonly Key Scroll      = new(KeyCodes.Scroll);
	public static readonly Key ScrollLock  = Scroll;

	/// <summary>
	/// Pause the current state or application (as appropriate).
	/// Do not use this value for the Pause button on media controllers. Use "MediaPause" instead.
	/// </summary>
	public static readonly Key Pause = new(KeyCodes.Pause);

	public static readonly Key Break            = Pause;
	public static readonly Key Num              = new(KeyCodes.NumLock);
	public static readonly Key NumLock          = Num;
	public static readonly Key Num0             = new(KeyCodes.NumPad0);
	public static readonly Key Num1             = new(KeyCodes.NumPad1);
	public static readonly Key Num2             = new(KeyCodes.NumPad2);
	public static readonly Key Num3             = new(KeyCodes.NumPad3);
	public static readonly Key Num4             = new(KeyCodes.NumPad4);
	public static readonly Key Num5             = new(KeyCodes.NumPad5);
	public static readonly Key Num6             = new(KeyCodes.NumPad6);
	public static readonly Key Num7             = new(KeyCodes.NumPad7);
	public static readonly Key Num8             = new(KeyCodes.NumPad8);
	public static readonly Key Num9             = new(KeyCodes.NumPad9);
	public static readonly Key Decimal          = new(KeyCodes.Decimal);
	public static readonly Key Multiply         = new(KeyCodes.Multiply);
	public static readonly Key Add              = new(KeyCodes.Add);
	public static readonly Key Separator        = new(KeyCodes.Separator);
	public static readonly Key Divide           = new(KeyCodes.Divide);
	public static readonly Key Subtract         = new(KeyCodes.Subtract);
	public static readonly Key Sleep            = new(KeyCodes.Sleep);
	public static readonly Key MediaPlayPause   = new(KeyCodes.MediaPlayPause);
	public static readonly Key MediaStop        = new(KeyCodes.MediaStop);
	public static readonly Key MediaNext        = new(KeyCodes.MediaNextTrack);
	public static readonly Key MediaPrevious    = new(KeyCodes.MediaPreviousTrack);
	public static readonly Key SelectMedia      = new(KeyCodes.SelectMedia);
	public static readonly Key VolumeDown       = new(KeyCodes.VolumeDown);
	public static readonly Key VolumeUp         = new(KeyCodes.VolumeUp);
	public static readonly Key VolumeMute       = new(KeyCodes.VolumeMute);
	public static readonly Key BrowserRefresh   = new(KeyCodes.BrowserRefresh);
	public static readonly Key BrowserBack      = new(KeyCodes.BrowserBack);
	public static readonly Key BrowserForward   = new(KeyCodes.BrowserForward);
	public static readonly Key BrowserHome      = new(KeyCodes.BrowserHome);
	public static readonly Key BrowserSearch    = new(KeyCodes.BrowserSearch);
	public static readonly Key BrowserFavorites = new(KeyCodes.BrowserFavorites);
	public static readonly Key Cancel           = new(KeyCodes.Cancel);
	public static readonly Key LineFeed         = new(KeyCodes.LineFeed);
	public static readonly Key LButton          = new(KeyCodes.LButton);
	public static readonly Key RButton          = new(KeyCodes.RButton);
	public static readonly Key MButton          = new(KeyCodes.MButton);
	public static readonly Key XButton1         = new(KeyCodes.XButton1);
	public static readonly Key XButton2         = new(KeyCodes.XButton2);
	public static readonly Key Mail             = new(KeyCodes.LaunchMail);
	public static readonly Key App1             = new(KeyCodes.LaunchApplication1);
	public static readonly Key App2             = new(KeyCodes.LaunchApplication2);
	public static readonly Key Email            = Mail;

	/// <summary>
	/// The Cursor Select (Crsel) key.
	/// </summary>
	public static readonly Key Crsel = new(KeyCodes.Crsel);

	/// <summary>
	/// Remove the currently selected input.
	/// </summary>
	public static readonly Key Clear = new(KeyCodes.Clear);

	/// <summary>
	/// The Extend Selection (Exsel) key.
	/// </summary>
	public static readonly Key Exsel = new(KeyCodes.Exsel);

	/// <summary>
	/// The Erase to End of Field key. This key deletes all characters from the current cursor position to the end of the current field.
	/// </summary>
	public static readonly Key EraseEof = new(KeyCodes.EraseEof);

	/// <summary>
	/// The Attention (Attn) key.
	/// </summary>
	public static readonly Key Attn = new(KeyCodes.Attn);

	/// <summary>
	/// Open a help dialog or toggle display of help information. (APPCOMMAND_HELP, KEYCODE_HELP)
	/// </summary>
	public static readonly Key Help = new(KeyCodes.Help);

	/// <summary>
	/// Play or resume the current state or application (as appropriate).
	/// Do not use this value for the Play button on media controllers. Use "MediaPlay" instead.
	/// </summary>
	public static readonly Key Play = new(KeyCodes.Play);

	public static readonly Key Select = new(KeyCodes.Select);

	/// <summary>
	/// Print the current document or message. (APPCOMMAND_PRINT)
	/// </summary>
	public static readonly Key Print = new(KeyCodes.Print);

	public static readonly Key Zoom = new(KeyCodes.Zoom);

	/// <summary>
	/// Used to pass Unicode characters as if they were keystrokes. The Packet key value is the low word of a 32-bit virtual-key value used for non-keyboard input methods
	/// </summary>
	public static readonly Key Packet = new(KeyCodes.Packet);

	public static readonly Key Pa1           = new(KeyCodes.Pa1);
	public static readonly Key NoName        = new(KeyCodes.NoName);
	public static readonly Key HangulMode    = new(KeyCodes.HangulMode);
	public static readonly Key KanaMode      = new(KeyCodes.KanaMode);
	public static readonly Key HanguelMode   = new(KeyCodes.HanguelMode);
	public static readonly Key JunjaMode     = new(KeyCodes.JunjaMode);
	public static readonly Key FinalMode     = new(KeyCodes.FinalMode);
	public static readonly Key HanjaMode     = new(KeyCodes.HanjaMode);
	public static readonly Key KanjiMode     = new(KeyCodes.KanjiMode);
	public static readonly Key IMEConvert    = new(KeyCodes.IMEConvert);
	public static readonly Key IMENonconvert = new(KeyCodes.IMENonconvert);
	public static readonly Key IMEAccept     = new(KeyCodes.IMEAccept);
	public static readonly Key IMEModeChange = new(KeyCodes.IMEModeChange);

	public static readonly Key[] CommonChordKeys = new Key[]
		{LCtrl, RCtrl, CtrlKey, LShift, RShift, ShiftKey, LMenu, RMenu, Menu, LWin, RWin, Win};

	public bool IsCommonChordKey()
	{
		return CommonChordKeys.Any(chord => this == chord);
	}

	private static readonly IDictionary<string, Key> _all;

	public static readonly IDictionary<string, Key> All = _all ??= typeof(Key)
		.GetFields(BindingFlags.Static | BindingFlags.Public)
		.Where(f => typeof(Key).IsAssignableFrom(f.FieldType))
		.ToDictionary(f => f.Name, fi => fi.GetValue(null) as Key);

	private static readonly IEnumerable<Key> _allKeys;

	public static readonly  IEnumerable<Key>    AllKeys = _allKeys??=All.Values;
	private static readonly IEnumerable<string> _allNames;

	public static readonly IEnumerable<string> AllNames = _allNames ??= All.Keys;
}