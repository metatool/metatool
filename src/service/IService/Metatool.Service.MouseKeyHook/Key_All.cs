using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Metatool.Service
{
    public partial class Key
    {
        // const                  Keys AnyKeyCode    = Keys.KeyCode;
        public static readonly Key None = new Key(Keys.None);

        // public static readonly Key  Any           = new Key(AnyKeyCode);
        public static readonly Key Caps          = new Key(Keys.CapsLock);
        public static readonly Key CapsLock      = Caps;
        public static readonly Key LCtrl         = new Key(Keys.LControlKey);
        public static readonly Key RCtrl         = new Key(Keys.RControlKey);
        public static readonly Key CtrlKey       = new Key(Keys.ControlKey);
        public static readonly Key Ctrl          = new Key(Keys.LControlKey, Keys.RControlKey);
        public static readonly Key CtrlChord     = new Key(Keys.Control);
        public static readonly Key LShift        = new Key(Keys.LShiftKey);
        public static readonly Key RShift        = new Key(Keys.RShiftKey);
        public static readonly Key ShiftKey      = new Key(Keys.ShiftKey);
        public static readonly Key Shift         = new Key(Keys.LShiftKey, Keys.RShiftKey);
        public static readonly Key ShiftChord    = new Key(Keys.Shift);
        public static readonly Key LAlt          = new Key(Keys.LMenu);
        public static readonly Key LMenu         = LAlt;
        public static readonly Key RAlt          = new Key(Keys.RMenu);
        public static readonly Key RMenu         = RAlt;
        public static readonly Key AltKey        = new Key(Keys.Menu);
        public static readonly Key Alt           = new Key(Keys.LMenu, Keys.RMenu);
        public static readonly Key Menu          = AltKey;
        public static readonly Key AltChord      = new Key(Keys.Alt);
        public static readonly Key Enter         = new Key(Keys.Enter);
        public static readonly Key Return        = Enter;
        public static readonly Key Tab           = new Key(Keys.Tab);
        public static readonly Key LWin          = new Key(Keys.LWin);
        public static readonly Key RWin          = new Key(Keys.RWin);
        public static readonly Key Win           = new Key(Keys.LWin, Keys.RWin);
        public static readonly Key Apps          = new Key(Keys.Apps);
        public static readonly Key Back          = new Key(Keys.Back);
        public static readonly Key Backspace     = Back;
        public static readonly Key Space         = new Key(Keys.Space);
        public static readonly Key SpaceBar      = Space;
        public static readonly Key A             = new Key(Keys.A);
        public static readonly Key B             = new Key(Keys.B);
        public static readonly Key C             = new Key(Keys.C);
        public static readonly Key D             = new Key(Keys.D);
        public static readonly Key E             = new Key(Keys.E);
        public static readonly Key F             = new Key(Keys.F);
        public static readonly Key G             = new Key(Keys.G);
        public static readonly Key H             = new Key(Keys.H);
        public static readonly Key I             = new Key(Keys.I);
        public static readonly Key J             = new Key(Keys.J);
        public static readonly Key K             = new Key(Keys.K);
        public static readonly Key L             = new Key(Keys.L);
        public static readonly Key M             = new Key(Keys.M);
        public static readonly Key N             = new Key(Keys.N);
        public static readonly Key O             = new Key(Keys.O);
        public static readonly Key P             = new Key(Keys.P);
        public static readonly Key Q             = new Key(Keys.Q);
        public static readonly Key R             = new Key(Keys.R);
        public static readonly Key S             = new Key(Keys.S);
        public static readonly Key T             = new Key(Keys.T);
        public static readonly Key U             = new Key(Keys.U);
        public static readonly Key V             = new Key(Keys.V);
        public static readonly Key W             = new Key(Keys.W);
        public static readonly Key X             = new Key(Keys.X);
        public static readonly Key Y             = new Key(Keys.Y);
        public static readonly Key Z             = new Key(Keys.Z);
        public static readonly Key Period        = new Key(Keys.OemPeriod);
        public static readonly Key Comma         = new Key(Keys.Oemcomma);
        public static readonly Key Question      = new Key(Keys.OemQuestion);
        public static readonly Key Slash         = Question;
        public static readonly Key Quotes        = new Key(Keys.OemQuotes);
        public static readonly Key SemiColon     = new Key(Keys.OemSemicolon);
        public static readonly Key OpenBrackets  = new Key(Keys.OemOpenBrackets);
        public static readonly Key CloseBrackets = new Key(Keys.OemCloseBrackets);

        /// <summary>
        /// On US keyboard please use Pipe
        /// </summary>
        public static readonly Key Backslash = new Key(Keys.OemBackslash);

        /// <summary>
        /// Blackslash and Pipe on US keyboard
        /// </summary>
        public static readonly Key Pipe = new Key(Keys.OemPipe);

        public static readonly Key Esc         = new Key(Keys.Escape);
        public static readonly Key Tilde       = new Key(Keys.Oemtilde);
        public static readonly Key D1          = new Key(Keys.D1);
        public static readonly Key D2          = new Key(Keys.D2);
        public static readonly Key D3          = new Key(Keys.D3);
        public static readonly Key D4          = new Key(Keys.D4);
        public static readonly Key D5          = new Key(Keys.D5);
        public static readonly Key D6          = new Key(Keys.D6);
        public static readonly Key D7          = new Key(Keys.D7);
        public static readonly Key D8          = new Key(Keys.D8);
        public static readonly Key D9          = new Key(Keys.D9);
        public static readonly Key D0          = new Key(Keys.D0);
        public static readonly Key Minus       = new Key(Keys.OemMinus);
        public static readonly Key Plus        = new Key(Keys.Oemplus);
        public static readonly Key F1          = new Key(Keys.F1);
        public static readonly Key F2          = new Key(Keys.F2);
        public static readonly Key F3          = new Key(Keys.F3);
        public static readonly Key F4          = new Key(Keys.F4);
        public static readonly Key F5          = new Key(Keys.F5);
        public static readonly Key F6          = new Key(Keys.F6);
        public static readonly Key F7          = new Key(Keys.F7);
        public static readonly Key F8          = new Key(Keys.F8);
        public static readonly Key F9          = new Key(Keys.F9);
        public static readonly Key F10         = new Key(Keys.F10);
        public static readonly Key F11         = new Key(Keys.F11);
        public static readonly Key F12         = new Key(Keys.F12);
        public static readonly Key Ins         = new Key(Keys.Insert);
        public static readonly Key Insert = Ins;
        public static readonly Key Del         = new Key(Keys.Delete);
        public static readonly Key Home        = new Key(Keys.Home);
        public static readonly Key End         = new Key(Keys.End);
        public static readonly Key Up          = new Key(Keys.Up);
        public static readonly Key Down        = new Key(Keys.Down);
        public static readonly Key Left        = new Key(Keys.Left);
        public static readonly Key Right       = new Key(Keys.Right);
        public static readonly Key PageUp      = new Key(Keys.PageUp);
        public static readonly Key Prior       = PageUp;
        public static readonly Key PageDown    = new Key(Keys.PageDown);
        public static readonly Key Next        = PageDown;
        public static readonly Key PrintScreen = new Key(Keys.PrintScreen);
        public static readonly Key Snapshot    = PrintScreen;
        public static readonly Key Scroll      = new Key(Keys.Scroll);
        public static readonly Key ScrollLock  = Scroll;

        /// <summary>
        /// Pause the current state or application (as appropriate).
        /// Do not use this value for the Pause button on media controllers. Use "MediaPause" instead.
        /// </summary>
        public static readonly Key Pause = new Key(Keys.Pause);

        public static readonly Key Break            = Pause;
        public static readonly Key Num              = new Key(Keys.NumLock);
        public static readonly Key NumLock          = Num;
        public static readonly Key Num0             = new Key(Keys.NumPad0);
        public static readonly Key Num1             = new Key(Keys.NumPad1);
        public static readonly Key Num2             = new Key(Keys.NumPad2);
        public static readonly Key Num3             = new Key(Keys.NumPad3);
        public static readonly Key Num4             = new Key(Keys.NumPad4);
        public static readonly Key Num5             = new Key(Keys.NumPad5);
        public static readonly Key Num6             = new Key(Keys.NumPad6);
        public static readonly Key Num7             = new Key(Keys.NumPad7);
        public static readonly Key Num8             = new Key(Keys.NumPad8);
        public static readonly Key Num9             = new Key(Keys.NumPad9);
        public static readonly Key Decimal          = new Key(Keys.Decimal);
        public static readonly Key Multiply         = new Key(Keys.Multiply);
        public static readonly Key Add              = new Key(Keys.Add);
        public static readonly Key Separator        = new Key(Keys.Separator);
        public static readonly Key Divide           = new Key(Keys.Divide);
        public static readonly Key Subtract         = new Key(Keys.Subtract);
        public static readonly Key Sleep            = new Key(Keys.Sleep);
        public static readonly Key MediaPlayPause   = new Key(Keys.MediaPlayPause);
        public static readonly Key MediaStop        = new Key(Keys.MediaStop);
        public static readonly Key MediaNext        = new Key(Keys.MediaNextTrack);
        public static readonly Key MediaPrevious    = new Key(Keys.MediaPreviousTrack);
        public static readonly Key SelectMedia      = new Key(Keys.SelectMedia);
        public static readonly Key VolumeDown       = new Key(Keys.VolumeDown);
        public static readonly Key VolumeUp         = new Key(Keys.VolumeUp);
        public static readonly Key VolumeMute       = new Key(Keys.VolumeMute);
        public static readonly Key BrowserRefresh   = new Key(Keys.BrowserRefresh);
        public static readonly Key BrowserBack      = new Key(Keys.BrowserBack);
        public static readonly Key BrowserForward   = new Key(Keys.BrowserForward);
        public static readonly Key BrowserHome      = new Key(Keys.BrowserHome);
        public static readonly Key BrowserSearch    = new Key(Keys.BrowserSearch);
        public static readonly Key BrowserFavorites = new Key(Keys.BrowserFavorites);
        public static readonly Key Cancel           = new Key(Keys.Cancel);
        public static readonly Key LineFeed         = new Key(Keys.LineFeed);
        public static readonly Key LButton          = new Key(Keys.LButton);
        public static readonly Key RButton          = new Key(Keys.RButton);
        public static readonly Key MButton          = new Key(Keys.MButton);
        public static readonly Key XButton1         = new Key(Keys.XButton1);
        public static readonly Key XButton2         = new Key(Keys.XButton2);
        public static readonly Key Mail             = new Key(Keys.LaunchMail);
        public static readonly Key App1             = new Key(Keys.LaunchApplication1);
        public static readonly Key App2             = new Key(Keys.LaunchApplication2);
        public static readonly Key Email            = Mail;

        /// <summary>
        /// The Cursor Select (Crsel) key.
        /// </summary>
        public static readonly Key Crsel = new Key(Keys.Crsel);

        /// <summary>
        /// Remove the currently selected input.
        /// </summary>
        public static readonly Key Clear = new Key(Keys.Clear);

        /// <summary>
        /// The Extend Selection (Exsel) key.
        /// </summary>
        public static readonly Key Exsel = new Key(Keys.Exsel);

        /// <summary>
        /// The Erase to End of Field key. This key deletes all characters from the current cursor position to the end of the current field.
        /// </summary>
        public static readonly Key EraseEof = new Key(Keys.EraseEof);

        /// <summary>
        /// The Attention (Attn) key.
        /// </summary>
        public static readonly Key Attn = new Key(Keys.Attn);

        /// <summary>
        /// Open a help dialog or toggle display of help information. (APPCOMMAND_HELP, KEYCODE_HELP)
        /// </summary>
        public static readonly Key Help = new Key(Keys.Help);

        /// <summary>
        /// Play or resume the current state or application (as appropriate).
        /// Do not use this value for the Play button on media controllers. Use "MediaPlay" instead.
        /// </summary>
        public static readonly Key Play = new Key(Keys.Play);

        public static readonly Key Select = new Key(Keys.Select);

        /// <summary>
        /// Print the current document or message. (APPCOMMAND_PRINT)
        /// </summary>
        public static readonly Key Print = new Key(Keys.Print);

        public static readonly Key Zoom = new Key(Keys.Zoom);

        /// <summary>
        /// Used to pass Unicode characters as if they were keystrokes. The Packet key value is the low word of a 32-bit virtual-key value used for non-keyboard input methods
        /// </summary>
        public static readonly Key Packet = new Key(Keys.Packet);

        public static readonly Key Pa1           = new Key(Keys.Pa1);
        public static readonly Key NoName        = new Key(Keys.NoName);
        public static readonly Key HangulMode    = new Key(Keys.HangulMode);
        public static readonly Key KanaMode      = new Key(Keys.KanaMode);
        public static readonly Key HanguelMode   = new Key(Keys.HanguelMode);
        public static readonly Key JunjaMode     = new Key(Keys.JunjaMode);
        public static readonly Key FinalMode     = new Key(Keys.FinalMode);
        public static readonly Key HanjaMode     = new Key(Keys.HanjaMode);
        public static readonly Key KanjiMode     = new Key(Keys.KanjiMode);
        public static readonly Key IMEConvert    = new Key(Keys.IMEConvert);
        public static readonly Key IMENonconvert = new Key(Keys.IMENonconvert);
        public static readonly Key IMEAccept     = new Key(Keys.IMEAccept);
        public static readonly Key IMEModeChange = new Key(Keys.IMEModeChange);

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
}