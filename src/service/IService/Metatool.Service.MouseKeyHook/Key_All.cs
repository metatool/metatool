using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Metatool.Service;

public partial class Key
{
	// const                  Keys AnyKeyCode    = Keys.KeyCode;
	public static readonly Key None = new(Keys.None);

	// public static readonly Key  Any           = new Key(AnyKeyCode);
	public static readonly Key Caps          = new(Keys.CapsLock);
	public static readonly Key CapsLock      = Caps;
	public static readonly Key LCtrl         = new(Keys.LControlKey);
	public static readonly Key RCtrl         = new(Keys.RControlKey);
	public static readonly Key CtrlKey       = new(Keys.ControlKey);
	public static readonly Key Ctrl          = new(Keys.LControlKey, Keys.RControlKey);
	public static readonly Key CtrlChord     = new(Keys.Control);
	public static readonly Key LShift        = new(Keys.LShiftKey);
	public static readonly Key RShift        = new(Keys.RShiftKey);
	public static readonly Key ShiftKey      = new(Keys.ShiftKey);
	public static readonly Key Shift         = new(Keys.LShiftKey, Keys.RShiftKey);
	public static readonly Key ShiftChord    = new(Keys.Shift);
	public static readonly Key LAlt          = new(Keys.LMenu);
	public static readonly Key LMenu         = LAlt;
	public static readonly Key RAlt          = new(Keys.RMenu);
	public static readonly Key RMenu         = RAlt;
	public static readonly Key AltKey        = new(Keys.Menu);
	public static readonly Key Alt           = new(Keys.LMenu, Keys.RMenu);
	public static readonly Key Menu          = AltKey;
	public static readonly Key AltChord      = new(Keys.Alt);
	public static readonly Key Enter         = new(Keys.Enter);
	public static readonly Key Return        = Enter;
	public static readonly Key Tab           = new(Keys.Tab);
	public static readonly Key LWin          = new(Keys.LWin);
	public static readonly Key RWin          = new(Keys.RWin);
	public static readonly Key Win           = new(Keys.LWin, Keys.RWin);
	public static readonly Key Apps          = new(Keys.Apps);
	public static readonly Key Back          = new(Keys.Back);
	public static readonly Key Backspace     = Back;
	public static readonly Key BS            = Back;
	public static readonly Key Space         = new(Keys.Space);
	public static readonly Key SpaceBar      = Space;
	public static readonly Key A             = new(Keys.A);
	public static readonly Key B             = new(Keys.B);
	public static readonly Key C             = new(Keys.C);
	public static readonly Key D             = new(Keys.D);
	public static readonly Key E             = new(Keys.E);
	public static readonly Key F             = new(Keys.F);
	public static readonly Key G             = new(Keys.G);
	public static readonly Key H             = new(Keys.H);
	public static readonly Key I             = new(Keys.I);
	public static readonly Key J             = new(Keys.J);
	public static readonly Key K             = new(Keys.K);
	public static readonly Key L             = new(Keys.L);
	public static readonly Key M             = new(Keys.M);
	public static readonly Key N             = new(Keys.N);
	public static readonly Key O             = new(Keys.O);
	public static readonly Key P             = new(Keys.P);
	public static readonly Key Q             = new(Keys.Q);
	public static readonly Key R             = new(Keys.R);
	public static readonly Key S             = new(Keys.S);
	public static readonly Key T             = new(Keys.T);
	public static readonly Key U             = new(Keys.U);
	public static readonly Key V             = new(Keys.V);
	public static readonly Key W             = new(Keys.W);
	public static readonly Key X             = new(Keys.X);
	public static readonly Key Y             = new(Keys.Y);
	public static readonly Key Z             = new(Keys.Z);
	public static readonly Key Period        = new(Keys.OemPeriod);
	public static readonly Key Comma         = new(Keys.Oemcomma);
	public static readonly Key Question      = new(Keys.OemQuestion);
	public static readonly Key Slash         = Question;
	public static readonly Key Quotes        = new(Keys.OemQuotes);
	public static readonly Key SemiColon     = new(Keys.OemSemicolon);
	public static readonly Key OpenBrackets  = new(Keys.OemOpenBrackets);
	public static readonly Key CloseBrackets = new(Keys.OemCloseBrackets);

	/// <summary>
	/// On US keyboard please use Pipe
	/// </summary>
	public static readonly Key Backslash = new(Keys.OemBackslash);

	/// <summary>
	/// Blackslash and Pipe on US keyboard
	/// </summary>
	public static readonly Key Pipe = new(Keys.OemPipe);

	public static readonly Key Esc         = new(Keys.Escape);
	public static readonly Key Tilde       = new(Keys.Oemtilde);
	public static readonly Key D1          = new(Keys.D1);
	public static readonly Key D2          = new(Keys.D2);
	public static readonly Key D3          = new(Keys.D3);
	public static readonly Key D4          = new(Keys.D4);
	public static readonly Key D5          = new(Keys.D5);
	public static readonly Key D6          = new(Keys.D6);
	public static readonly Key D7          = new(Keys.D7);
	public static readonly Key D8          = new(Keys.D8);
	public static readonly Key D9          = new(Keys.D9);
	public static readonly Key D0          = new(Keys.D0);
	public static readonly Key Minus       = new(Keys.OemMinus);
	public static readonly Key Plus        = new(Keys.Oemplus);
	public static readonly Key F1          = new(Keys.F1);
	public static readonly Key F2          = new(Keys.F2);
	public static readonly Key F3          = new(Keys.F3);
	public static readonly Key F4          = new(Keys.F4);
	public static readonly Key F5          = new(Keys.F5);
	public static readonly Key F6          = new(Keys.F6);
	public static readonly Key F7          = new(Keys.F7);
	public static readonly Key F8          = new(Keys.F8);
	public static readonly Key F9          = new(Keys.F9);
	public static readonly Key F10         = new(Keys.F10);
	public static readonly Key F11         = new(Keys.F11);
	public static readonly Key F12         = new(Keys.F12);
	public static readonly Key Ins         = new(Keys.Insert);
	public static readonly Key Insert      = Ins;
	public static readonly Key Del         = new(Keys.Delete);
	public static readonly Key Home        = new(Keys.Home);
	public static readonly Key End         = new(Keys.End);
	public static readonly Key Up          = new(Keys.Up);
	public static readonly Key Down        = new(Keys.Down);
	public static readonly Key Left        = new(Keys.Left);
	public static readonly Key Right       = new(Keys.Right);
	public static readonly Key PageUp      = new(Keys.PageUp);
	public static readonly Key Prior       = PageUp;
	public static readonly Key PageDown    = new(Keys.PageDown);
	public static readonly Key Next        = PageDown;
	public static readonly Key PrintScreen = new(Keys.PrintScreen);
	public static readonly Key Snapshot    = PrintScreen;
	public static readonly Key Scroll      = new(Keys.Scroll);
	public static readonly Key ScrollLock  = Scroll;

	/// <summary>
	/// Pause the current state or application (as appropriate).
	/// Do not use this value for the Pause button on media controllers. Use "MediaPause" instead.
	/// </summary>
	public static readonly Key Pause = new(Keys.Pause);

	public static readonly Key Break            = Pause;
	public static readonly Key Num              = new(Keys.NumLock);
	public static readonly Key NumLock          = Num;
	public static readonly Key Num0             = new(Keys.NumPad0);
	public static readonly Key Num1             = new(Keys.NumPad1);
	public static readonly Key Num2             = new(Keys.NumPad2);
	public static readonly Key Num3             = new(Keys.NumPad3);
	public static readonly Key Num4             = new(Keys.NumPad4);
	public static readonly Key Num5             = new(Keys.NumPad5);
	public static readonly Key Num6             = new(Keys.NumPad6);
	public static readonly Key Num7             = new(Keys.NumPad7);
	public static readonly Key Num8             = new(Keys.NumPad8);
	public static readonly Key Num9             = new(Keys.NumPad9);
	public static readonly Key Decimal          = new(Keys.Decimal);
	public static readonly Key Multiply         = new(Keys.Multiply);
	public static readonly Key Add              = new(Keys.Add);
	public static readonly Key Separator        = new(Keys.Separator);
	public static readonly Key Divide           = new(Keys.Divide);
	public static readonly Key Subtract         = new(Keys.Subtract);
	public static readonly Key Sleep            = new(Keys.Sleep);
	public static readonly Key MediaPlayPause   = new(Keys.MediaPlayPause);
	public static readonly Key MediaStop        = new(Keys.MediaStop);
	public static readonly Key MediaNext        = new(Keys.MediaNextTrack);
	public static readonly Key MediaPrevious    = new(Keys.MediaPreviousTrack);
	public static readonly Key SelectMedia      = new(Keys.SelectMedia);
	public static readonly Key VolumeDown       = new(Keys.VolumeDown);
	public static readonly Key VolumeUp         = new(Keys.VolumeUp);
	public static readonly Key VolumeMute       = new(Keys.VolumeMute);
	public static readonly Key BrowserRefresh   = new(Keys.BrowserRefresh);
	public static readonly Key BrowserBack      = new(Keys.BrowserBack);
	public static readonly Key BrowserForward   = new(Keys.BrowserForward);
	public static readonly Key BrowserHome      = new(Keys.BrowserHome);
	public static readonly Key BrowserSearch    = new(Keys.BrowserSearch);
	public static readonly Key BrowserFavorites = new(Keys.BrowserFavorites);
	public static readonly Key Cancel           = new(Keys.Cancel);
	public static readonly Key LineFeed         = new(Keys.LineFeed);
	public static readonly Key LButton          = new(Keys.LButton);
	public static readonly Key RButton          = new(Keys.RButton);
	public static readonly Key MButton          = new(Keys.MButton);
	public static readonly Key XButton1         = new(Keys.XButton1);
	public static readonly Key XButton2         = new(Keys.XButton2);
	public static readonly Key Mail             = new(Keys.LaunchMail);
	public static readonly Key App1             = new(Keys.LaunchApplication1);
	public static readonly Key App2             = new(Keys.LaunchApplication2);
	public static readonly Key Email            = Mail;

	/// <summary>
	/// The Cursor Select (Crsel) key.
	/// </summary>
	public static readonly Key Crsel = new(Keys.Crsel);

	/// <summary>
	/// Remove the currently selected input.
	/// </summary>
	public static readonly Key Clear = new(Keys.Clear);

	/// <summary>
	/// The Extend Selection (Exsel) key.
	/// </summary>
	public static readonly Key Exsel = new(Keys.Exsel);

	/// <summary>
	/// The Erase to End of Field key. This key deletes all characters from the current cursor position to the end of the current field.
	/// </summary>
	public static readonly Key EraseEof = new(Keys.EraseEof);

	/// <summary>
	/// The Attention (Attn) key.
	/// </summary>
	public static readonly Key Attn = new(Keys.Attn);

	/// <summary>
	/// Open a help dialog or toggle display of help information. (APPCOMMAND_HELP, KEYCODE_HELP)
	/// </summary>
	public static readonly Key Help = new(Keys.Help);

	/// <summary>
	/// Play or resume the current state or application (as appropriate).
	/// Do not use this value for the Play button on media controllers. Use "MediaPlay" instead.
	/// </summary>
	public static readonly Key Play = new(Keys.Play);

	public static readonly Key Select = new(Keys.Select);

	/// <summary>
	/// Print the current document or message. (APPCOMMAND_PRINT)
	/// </summary>
	public static readonly Key Print = new(Keys.Print);

	public static readonly Key Zoom = new(Keys.Zoom);

	/// <summary>
	/// Used to pass Unicode characters as if they were keystrokes. The Packet key value is the low word of a 32-bit virtual-key value used for non-keyboard input methods
	/// </summary>
	public static readonly Key Packet = new(Keys.Packet);

	public static readonly Key Pa1           = new(Keys.Pa1);
	public static readonly Key NoName        = new(Keys.NoName);
	public static readonly Key HangulMode    = new(Keys.HangulMode);
	public static readonly Key KanaMode      = new(Keys.KanaMode);
	public static readonly Key HanguelMode   = new(Keys.HanguelMode);
	public static readonly Key JunjaMode     = new(Keys.JunjaMode);
	public static readonly Key FinalMode     = new(Keys.FinalMode);
	public static readonly Key HanjaMode     = new(Keys.HanjaMode);
	public static readonly Key KanjiMode     = new(Keys.KanjiMode);
	public static readonly Key IMEConvert    = new(Keys.IMEConvert);
	public static readonly Key IMENonconvert = new(Keys.IMENonconvert);
	public static readonly Key IMEAccept     = new(Keys.IMEAccept);
	public static readonly Key IMEModeChange = new(Keys.IMEModeChange);

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