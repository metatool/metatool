using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metatool.Service.MouseKey;

public partial class Key
{
	// const                  Keys AnyKeyCode    = Keys.KeyCode;
	public static readonly Key None = new(KeyValues.None);

	// public static readonly Key  Any           = new Key(AnyKeyCode);
	public static readonly Key Caps          = new(KeyValues.CapsLock);
	public static readonly Key CapsLock      = Caps;
	public static readonly Key LCtrl         = new(KeyValues.LControlKey);
	public static readonly Key RCtrl         = new(KeyValues.RControlKey);
	public static readonly Key CtrlKey       = new(KeyValues.ControlKey);
	public static readonly Key Ctrl          = new(KeyValues.LControlKey, KeyValues.RControlKey);
	public static readonly Key CtrlChord     = new(KeyValues.Control);
	public static readonly Key LShift        = new(KeyValues.LShiftKey);
	public static readonly Key RShift        = new(KeyValues.RShiftKey);
	public static readonly Key ShiftKey      = new(KeyValues.ShiftKey);
	public static readonly Key Shift         = new(KeyValues.LShiftKey, KeyValues.RShiftKey);
	public static readonly Key ShiftChord    = new(KeyValues.Shift);
	public static readonly Key LAlt          = new(KeyValues.LMenu);
	public static readonly Key LMenu         = LAlt;
	public static readonly Key RAlt          = new(KeyValues.RMenu);
	public static readonly Key RMenu         = RAlt;
	public static readonly Key AltKey        = new(KeyValues.Menu);
	public static readonly Key Alt           = new(KeyValues.LMenu, KeyValues.RMenu);
	public static readonly Key Menu          = AltKey;
	public static readonly Key AltChord      = new(KeyValues.Alt);
	public static readonly Key Enter         = new(KeyValues.Enter);
	public static readonly Key Return        = Enter;
	public static readonly Key Tab           = new(KeyValues.Tab);
	public static readonly Key LWin          = new(KeyValues.LWin);
	public static readonly Key RWin          = new(KeyValues.RWin);
	public static readonly Key Win           = new(KeyValues.LWin, KeyValues.RWin);
	public static readonly Key Apps          = new(KeyValues.Apps);
	public static readonly Key Back          = new(KeyValues.Back);
	public static readonly Key Backspace     = Back;
	public static readonly Key BS            = Back;
	public static readonly Key Space         = new(KeyValues.Space);
	public static readonly Key SpaceBar      = Space;
	public static readonly Key A             = new(KeyValues.A);
	public static readonly Key B             = new(KeyValues.B);
	public static readonly Key C             = new(KeyValues.C);
	public static readonly Key D             = new(KeyValues.D);
	public static readonly Key E             = new(KeyValues.E);
	public static readonly Key F             = new(KeyValues.F);
	public static readonly Key G             = new(KeyValues.G);
	public static readonly Key H             = new(KeyValues.H);
	public static readonly Key I             = new(KeyValues.I);
	public static readonly Key J             = new(KeyValues.J);
	public static readonly Key K             = new(KeyValues.K);
	public static readonly Key L             = new(KeyValues.L);
	public static readonly Key M             = new(KeyValues.M);
	public static readonly Key N             = new(KeyValues.N);
	public static readonly Key O             = new(KeyValues.O);
	public static readonly Key P             = new(KeyValues.P);
	public static readonly Key Q             = new(KeyValues.Q);
	public static readonly Key R             = new(KeyValues.R);
	public static readonly Key S             = new(KeyValues.S);
	public static readonly Key T             = new(KeyValues.T);
	public static readonly Key U             = new(KeyValues.U);
	public static readonly Key V             = new(KeyValues.V);
	public static readonly Key W             = new(KeyValues.W);
	public static readonly Key X             = new(KeyValues.X);
	public static readonly Key Y             = new(KeyValues.Y);
	public static readonly Key Z             = new(KeyValues.Z);
	public static readonly Key Period        = new(KeyValues.OemPeriod);
	public static readonly Key Comma         = new(KeyValues.Oemcomma);
	public static readonly Key Question      = new(KeyValues.OemQuestion);
	public static readonly Key Slash         = Question;
	public static readonly Key Quotes        = new(KeyValues.OemQuotes);
	public static readonly Key SemiColon     = new(KeyValues.OemSemicolon);
	public static readonly Key OpenBrackets  = new(KeyValues.OemOpenBrackets);
	public static readonly Key CloseBrackets = new(KeyValues.OemCloseBrackets);

	/// <summary>
	/// On US keyboard please use Pipe
	/// </summary>
	public static readonly Key Backslash = new(KeyValues.OemBackslash);

	/// <summary>
	/// Blackslash and Pipe on US keyboard
	/// </summary>
	public static readonly Key Pipe = new(KeyValues.OemPipe);

	public static readonly Key Esc         = new(KeyValues.Escape);
	public static readonly Key Tilde       = new(KeyValues.Oemtilde);
	public static readonly Key D1          = new(KeyValues.D1);
	public static readonly Key D2          = new(KeyValues.D2);
	public static readonly Key D3          = new(KeyValues.D3);
	public static readonly Key D4          = new(KeyValues.D4);
	public static readonly Key D5          = new(KeyValues.D5);
	public static readonly Key D6          = new(KeyValues.D6);
	public static readonly Key D7          = new(KeyValues.D7);
	public static readonly Key D8          = new(KeyValues.D8);
	public static readonly Key D9          = new(KeyValues.D9);
	public static readonly Key D0          = new(KeyValues.D0);
	public static readonly Key Minus       = new(KeyValues.OemMinus);
	public static readonly Key Plus        = new(KeyValues.Oemplus);
	public static readonly Key F1          = new(KeyValues.F1);
	public static readonly Key F2          = new(KeyValues.F2);
	public static readonly Key F3          = new(KeyValues.F3);
	public static readonly Key F4          = new(KeyValues.F4);
	public static readonly Key F5          = new(KeyValues.F5);
	public static readonly Key F6          = new(KeyValues.F6);
	public static readonly Key F7          = new(KeyValues.F7);
	public static readonly Key F8          = new(KeyValues.F8);
	public static readonly Key F9          = new(KeyValues.F9);
	public static readonly Key F10         = new(KeyValues.F10);
	public static readonly Key F11         = new(KeyValues.F11);
	public static readonly Key F12         = new(KeyValues.F12);
	public static readonly Key Ins         = new(KeyValues.Insert);
	public static readonly Key Insert      = Ins;
	public static readonly Key Del         = new(KeyValues.Delete);
	public static readonly Key Home        = new(KeyValues.Home);
	public static readonly Key End         = new(KeyValues.End);
	public static readonly Key Up          = new(KeyValues.Up);
	public static readonly Key Down        = new(KeyValues.Down);
	public static readonly Key Left        = new(KeyValues.Left);
	public static readonly Key Right       = new(KeyValues.Right);
	public static readonly Key PageUp      = new(KeyValues.PageUp);
	public static readonly Key Prior       = PageUp;
	public static readonly Key PageDown    = new(KeyValues.PageDown);
	public static readonly Key Next        = PageDown;
	public static readonly Key PrintScreen = new(KeyValues.PrintScreen);
	public static readonly Key Snapshot    = PrintScreen;
	public static readonly Key Scroll      = new(KeyValues.Scroll);
	public static readonly Key ScrollLock  = Scroll;

	/// <summary>
	/// Pause the current state or application (as appropriate).
	/// Do not use this value for the Pause button on media controllers. Use "MediaPause" instead.
	/// </summary>
	public static readonly Key Pause = new(KeyValues.Pause);

	public static readonly Key Break            = Pause;
	public static readonly Key Num              = new(KeyValues.NumLock);
	public static readonly Key NumLock          = Num;
	public static readonly Key Num0             = new(KeyValues.NumPad0);
	public static readonly Key Num1             = new(KeyValues.NumPad1);
	public static readonly Key Num2             = new(KeyValues.NumPad2);
	public static readonly Key Num3             = new(KeyValues.NumPad3);
	public static readonly Key Num4             = new(KeyValues.NumPad4);
	public static readonly Key Num5             = new(KeyValues.NumPad5);
	public static readonly Key Num6             = new(KeyValues.NumPad6);
	public static readonly Key Num7             = new(KeyValues.NumPad7);
	public static readonly Key Num8             = new(KeyValues.NumPad8);
	public static readonly Key Num9             = new(KeyValues.NumPad9);
	public static readonly Key Decimal          = new(KeyValues.Decimal);
	public static readonly Key Multiply         = new(KeyValues.Multiply);
	public static readonly Key Add              = new(KeyValues.Add);
	public static readonly Key Separator        = new(KeyValues.Separator);
	public static readonly Key Divide           = new(KeyValues.Divide);
	public static readonly Key Subtract         = new(KeyValues.Subtract);
	public static readonly Key Sleep            = new(KeyValues.Sleep);
	public static readonly Key MediaPlayPause   = new(KeyValues.MediaPlayPause);
	public static readonly Key MediaStop        = new(KeyValues.MediaStop);
	public static readonly Key MediaNext        = new(KeyValues.MediaNextTrack);
	public static readonly Key MediaPrevious    = new(KeyValues.MediaPreviousTrack);
	public static readonly Key SelectMedia      = new(KeyValues.SelectMedia);
	public static readonly Key VolumeDown       = new(KeyValues.VolumeDown);
	public static readonly Key VolumeUp         = new(KeyValues.VolumeUp);
	public static readonly Key VolumeMute       = new(KeyValues.VolumeMute);
	public static readonly Key BrowserRefresh   = new(KeyValues.BrowserRefresh);
	public static readonly Key BrowserBack      = new(KeyValues.BrowserBack);
	public static readonly Key BrowserForward   = new(KeyValues.BrowserForward);
	public static readonly Key BrowserHome      = new(KeyValues.BrowserHome);
	public static readonly Key BrowserSearch    = new(KeyValues.BrowserSearch);
	public static readonly Key BrowserFavorites = new(KeyValues.BrowserFavorites);
	public static readonly Key Cancel           = new(KeyValues.Cancel);
	public static readonly Key LineFeed         = new(KeyValues.LineFeed);
	public static readonly Key LButton          = new(KeyValues.LButton);
	public static readonly Key RButton          = new(KeyValues.RButton);
	public static readonly Key MButton          = new(KeyValues.MButton);
	public static readonly Key XButton1         = new(KeyValues.XButton1);
	public static readonly Key XButton2         = new(KeyValues.XButton2);
	public static readonly Key Mail             = new(KeyValues.LaunchMail);
	public static readonly Key App1             = new(KeyValues.LaunchApplication1);
	public static readonly Key App2             = new(KeyValues.LaunchApplication2);
	public static readonly Key Email            = Mail;

	/// <summary>
	/// The Cursor Select (Crsel) key.
	/// </summary>
	public static readonly Key Crsel = new(KeyValues.Crsel);

	/// <summary>
	/// Remove the currently selected input.
	/// </summary>
	public static readonly Key Clear = new(KeyValues.Clear);

	/// <summary>
	/// The Extend Selection (Exsel) key.
	/// </summary>
	public static readonly Key Exsel = new(KeyValues.Exsel);

	/// <summary>
	/// The Erase to End of Field key. This key deletes all characters from the current cursor position to the end of the current field.
	/// </summary>
	public static readonly Key EraseEof = new(KeyValues.EraseEof);

	/// <summary>
	/// The Attention (Attn) key.
	/// </summary>
	public static readonly Key Attn = new(KeyValues.Attn);

	/// <summary>
	/// Open a help dialog or toggle display of help information. (APPCOMMAND_HELP, KEYCODE_HELP)
	/// </summary>
	public static readonly Key Help = new(KeyValues.Help);

	/// <summary>
	/// Play or resume the current state or application (as appropriate).
	/// Do not use this value for the Play button on media controllers. Use "MediaPlay" instead.
	/// </summary>
	public static readonly Key Play = new(KeyValues.Play);

	public static readonly Key Select = new(KeyValues.Select);

	/// <summary>
	/// Print the current document or message. (APPCOMMAND_PRINT)
	/// </summary>
	public static readonly Key Print = new(KeyValues.Print);

	public static readonly Key Zoom = new(KeyValues.Zoom);

	/// <summary>
	/// Used to pass Unicode characters as if they were keystrokes. The Packet key value is the low word of a 32-bit virtual-key value used for non-keyboard input methods
	/// </summary>
	public static readonly Key Packet = new(KeyValues.Packet);

	public static readonly Key Pa1           = new(KeyValues.Pa1);
	public static readonly Key NoName        = new(KeyValues.NoName);
	public static readonly Key HangulMode    = new(KeyValues.HangulMode);
	public static readonly Key KanaMode      = new(KeyValues.KanaMode);
	public static readonly Key HanguelMode   = new(KeyValues.HanguelMode);
	public static readonly Key JunjaMode     = new(KeyValues.JunjaMode);
	public static readonly Key FinalMode     = new(KeyValues.FinalMode);
	public static readonly Key HanjaMode     = new(KeyValues.HanjaMode);
	public static readonly Key KanjiMode     = new(KeyValues.KanjiMode);
	public static readonly Key IMEConvert    = new(KeyValues.IMEConvert);
	public static readonly Key IMENonconvert = new(KeyValues.IMENonconvert);
	public static readonly Key IMEAccept     = new(KeyValues.IMEAccept);
	public static readonly Key IMEModeChange = new(KeyValues.IMEModeChange);

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