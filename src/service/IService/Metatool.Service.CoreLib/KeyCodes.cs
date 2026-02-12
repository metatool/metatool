using System;
using System.Reflection;

namespace Metatool.Service.MouseKey;

public class KeyAttribute(string key) : Attribute
{
    /// <summary>
    /// use friend name, i.e. used in key to type for user in tooltip
    /// </summary>
    public string KeyLetter { get; set; } = key;
}
// from System.Windows.Forms.Keys
[Flags]
public enum KeyCodes
{
    /// <summary>The bitmask to extract a key code from a key value.</summary>
    KeyCode = 0x0000FFFF, // 65535
    /// <summary>The bitmask to extract modifiers from a key value.</summary>
    Modifiers = -65536, // 0xFFFF0000
    /// <summary>No key pressed.</summary>
    None = 0,
    /// <summary>The left mouse button.</summary>
    LButton = 1,
    /// <summary>The right mouse button.</summary>
    RButton = 2,
    /// <summary>The CANCEL key.</summary>
    Cancel = RButton | LButton, // 0x00000003
    /// <summary>The middle mouse button (three-button mouse).</summary>
    MButton = 4,
    /// <summary>The first x mouse button (five-button mouse).</summary>
    XButton1 = MButton | LButton, // 0x00000005
    /// <summary>The second x mouse button (five-button mouse).</summary>
    XButton2 = MButton | RButton, // 0x00000006
    /// <summary>The BACKSPACE key.</summary>
    Back = 8,
    /// <summary>The TAB key.</summary>
    Tab = Back | LButton, // 0x00000009
    /// <summary>The LINEFEED key.</summary>
    LineFeed = Back | RButton, // 0x0000000A
    /// <summary>The CLEAR key.</summary>
    Clear = Back | MButton, // 0x0000000C
    /// <summary>The RETURN key.</summary>
    Return = Clear | LButton, // 0x0000000D
    /// <summary>The ENTER key.</summary>
    Enter = Return, // 0x0000000D
    /// <summary>The SHIFT key.</summary>
    [Key("Shift")]
    ShiftKey = 16, // 0x00000010
    /// <summary>The CTRL key.</summary>
    [Key("Ctrl")]
    ControlKey = ShiftKey | LButton, // 0x00000011
    /// <summary>The ALT key.</summary>
    [Key("Alt")]
    Menu = ShiftKey | RButton, // 0x00000012
    /// <summary>The PAUSE key.</summary>
    [Key("Pause")]
    Pause = Menu | LButton, // 0x00000013
    /// <summary>The CAPS LOCK key.</summary>
    [Key("Caps")]
    Capital = ShiftKey | MButton, // 0x00000014
    /// <summary>The CAPS LOCK key.</summary>
    [Key("Caps")]
    CapsLock = Capital, // 0x00000014
    /// <summary>The IME Kana mode key.</summary>
    KanaMode = CapsLock | LButton, // 0x00000015
    /// <summary>The IME Hanguel mode key. (maintained for compatibility; use <see langword="HangulMode" />)</summary>
    HanguelMode = KanaMode, // 0x00000015
    /// <summary>The IME Hangul mode key.</summary>
    HangulMode = HanguelMode, // 0x00000015
    /// <summary>The IME Junja mode key.</summary>
    JunjaMode = HangulMode | RButton, // 0x00000017
    /// <summary>The IME final mode key.</summary>
    FinalMode = ShiftKey | Back, // 0x00000018
    /// <summary>The IME Hanja mode key.</summary>
    HanjaMode = FinalMode | LButton, // 0x00000019
    /// <summary>The IME Kanji mode key.</summary>
    KanjiMode = HanjaMode, // 0x00000019
    /// <summary>The ESC key.</summary>
    Escape = KanjiMode | RButton, // 0x0000001B
    /// <summary>The IME convert key.</summary>
    IMEConvert = FinalMode | MButton, // 0x0000001C
    /// <summary>The IME nonconvert key.</summary>
    IMENonconvert = IMEConvert | LButton, // 0x0000001D
    /// <summary>The IME accept key, replaces <see cref="F:System.Windows.Forms.Keys.IMEAceept" />.</summary>
    IMEAccept = IMEConvert | RButton, // 0x0000001E
    /// <summary>The IME accept key. Obsolete, use <see cref="F:System.Windows.Forms.Keys.IMEAccept" /> instead.</summary>
    IMEAceept = IMEAccept, // 0x0000001E
    /// <summary>The IME mode change key.</summary>
    IMEModeChange = IMEAceept | LButton, // 0x0000001F
    /// <summary>The SPACEBAR key.</summary>
    Space = 32, // 0x00000020
    /// <summary>The PAGE UP key.</summary>
    PageUp = Space | LButton, // 0x00000021
    /// <summary>The PAGE UP key.</summary>
    [Key("PageUp")]
    Prior = PageUp, // 0x00000021
    /// <summary>The PAGE DOWN key.</summary>
    PageDown = Space | RButton, // 0x00000022
    /// <summary>The PAGE DOWN key.</summary>
    [Key("PageDown")]
    Next = PageDown, // 0x00000022
    /// <summary>The END key.</summary>
    End = PageDown | LButton, // 0x00000023
    /// <summary>The HOME key.</summary>
    Home = Space | MButton, // 0x00000024
    /// <summary>The LEFT ARROW key.</summary>
    Left = Home | LButton, // 0x00000025
    /// <summary>The UP ARROW key.</summary>
    Up = Home | RButton, // 0x00000026
    /// <summary>The RIGHT ARROW key.</summary>
    Right = Up | LButton, // 0x00000027
    /// <summary>The DOWN ARROW key.</summary>
    Down = Space | Back, // 0x00000028
    /// <summary>The SELECT key.</summary>
    Select = Down | LButton, // 0x00000029
    /// <summary>The PRINT key.</summary>
    Print = Down | RButton, // 0x0000002A
    /// <summary>The EXECUTE key.</summary>
    Execute = Print | LButton, // 0x0000002B
    /// <summary>The PRINT SCREEN key.</summary>
    Snapshot = Down | MButton, // 0x0000002C
    /// <summary>The PRINT SCREEN key.</summary>
    PrintScreen = Snapshot, // 0x0000002C
    /// <summary>The INS key.</summary>
    Insert = PrintScreen | LButton, // 0x0000002D
    /// <summary>The DEL key.</summary>
    Delete = PrintScreen | RButton, // 0x0000002E
    /// <summary>The HELP key.</summary>
    Help = Delete | LButton, // 0x0000002F
    /// <summary>The 0 key.</summary>
    [Key("0")]
    D0 = Space | ShiftKey, // 0x00000030
    /// <summary>The 1 key.</summary>
    [Key("1")]
    D1 = D0 | LButton, // 0x00000031
    /// <summary>The 2 key.</summary>
    [Key("2")]
    D2 = D0 | RButton, // 0x00000032
    /// <summary>The 3 key.</summary>
    [Key("3")]
    D3 = D2 | LButton, // 0x00000033
    /// <summary>The 4 key.</summary>
    [Key("4")]
    D4 = D0 | MButton, // 0x00000034
    /// <summary>The 5 key.</summary>
    [Key("5")]
    D5 = D4 | LButton, // 0x00000035
    /// <summary>The 6 key.</summary>
    [Key("6")]
    D6 = D4 | RButton, // 0x00000036
    /// <summary>The 7 key.</summary>
    [Key("7")]
    D7 = D6 | LButton, // 0x00000037
    /// <summary>The 8 key.</summary>
    [Key("8")]
    D8 = D0 | Back, // 0x00000038
    /// <summary>The 9 key.</summary>
    [Key("9")]
    D9 = D8 | LButton, // 0x00000039

    // 0x3A - 0x40 undefined

    /// <summary>The A key.</summary>
    [Key("a")]
    A = 65, // 0x00000041
    /// <summary>The B key.</summary>
    [Key("b")]
    B = 66, // 0x00000042
    /// <summary>The C key.</summary>
    [Key("c")]
    C = B | LButton, // 0x00000043
    /// <summary>The D key.</summary>
    [Key("d")]
    D = 68, // 0x00000044
    /// <summary>The E key.</summary>
    [Key("e")]
    E = D | LButton, // 0x00000045
    /// <summary>The F key.</summary>
    [Key("f")]
    F = D | RButton, // 0x00000046
    /// <summary>The G key.</summary>
    [Key("g")]
    G = F | LButton, // 0x00000047
    /// <summary>The H key.</summary>
    [Key("h")]
    H = 72, // 0x00000048
    /// <summary>The I key.</summary>
    [Key("i")]
    I = H | LButton, // 0x00000049
    /// <summary>The J key.</summary>
    [Key("j")]
    J = H | RButton, // 0x0000004A
    /// <summary>The K key.</summary>
    [Key("k")]
    K = J | LButton, // 0x0000004B
    /// <summary>The L key.</summary>
    [Key("l")]
    L = H | MButton, // 0x0000004C
    /// <summary>The M key.</summary>
    [Key("m")]
    M = L | LButton, // 0x0000004D
    /// <summary>The N key.</summary>
    [Key("n")]
    N = L | RButton, // 0x0000004E
    /// <summary>The O key.</summary>
    [Key("o")]
    O = N | LButton, // 0x0000004F
    /// <summary>The P key.</summary>
    [Key("p")]
    P = 80, // 0x00000050
    /// <summary>The Q key.</summary>
    [Key("q")]
    Q = P | LButton, // 0x00000051
    /// <summary>The R key.</summary>
    [Key("r")]
    R = P | RButton, // 0x00000052
    /// <summary>The S key.</summary>
    [Key("s")]
    S = R | LButton, // 0x00000053
    /// <summary>The T key.</summary>
    [Key("t")]
    T = P | MButton, // 0x00000054
    /// <summary>The U key.</summary>
    [Key("u")]
    U = T | LButton, // 0x00000055
    /// <summary>The V key.</summary>
    [Key("v")]
    V = T | RButton, // 0x00000056
    /// <summary>The W key.</summary>
    [Key("w")]
    W = V | LButton, // 0x00000057
    /// <summary>The X key.</summary>
    [Key("x")]
    X = P | Back, // 0x00000058
    /// <summary>The Y key.</summary>
    [Key("y")]
    Y = X | LButton, // 0x00000059
    /// <summary>The Z key.</summary>
    [Key("z")]
    Z = X | RButton, // 0x0000005A
    /// <summary>The left Windows logo key (Microsoft Natural Keyboard).</summary>
    LWin = Z | LButton, // 0x0000005B
    /// <summary>The right Windows logo key (Microsoft Natural Keyboard).</summary>
    RWin = X | MButton, // 0x0000005C
    /// <summary>The application key (Microsoft Natural Keyboard).</summary>
    Apps = RWin | LButton, // 0x0000005D

    // 0x5E reserved

    /// <summary>The computer sleep key.</summary>
    Sleep = Apps | RButton, // 0x0000005F
    /// <summary>The 0 key on the numeric keypad.</summary>
    NumPad0 = 96, // 0x00000060
    /// <summary>The 1 key on the numeric keypad.</summary>
    NumPad1 = NumPad0 | LButton, // 0x00000061
    /// <summary>The 2 key on the numeric keypad.</summary>
    NumPad2 = NumPad0 | RButton, // 0x00000062
    /// <summary>The 3 key on the numeric keypad.</summary>
    NumPad3 = NumPad2 | LButton, // 0x00000063
    /// <summary>The 4 key on the numeric keypad.</summary>
    NumPad4 = NumPad0 | MButton, // 0x00000064
    /// <summary>The 5 key on the numeric keypad.</summary>
    NumPad5 = NumPad4 | LButton, // 0x00000065
    /// <summary>The 6 key on the numeric keypad.</summary>
    NumPad6 = NumPad4 | RButton, // 0x00000066
    /// <summary>The 7 key on the numeric keypad.</summary>
    NumPad7 = NumPad6 | LButton, // 0x00000067
    /// <summary>The 8 key on the numeric keypad.</summary>
    NumPad8 = NumPad0 | Back, // 0x00000068
    /// <summary>The 9 key on the numeric keypad.</summary>
    NumPad9 = NumPad8 | LButton, // 0x00000069
    /// <summary>The multiply key.</summary>
    Multiply = NumPad8 | RButton, // 0x0000006A
    /// <summary>The add key.</summary>
    Add = Multiply | LButton, // 0x0000006B
    /// <summary>The separator key.</summary>
    Separator = NumPad8 | MButton, // 0x0000006C
    /// <summary>The subtract key.</summary>
    Subtract = Separator | LButton, // 0x0000006D
    /// <summary>The decimal key.</summary>
    Decimal = Separator | RButton, // 0x0000006E
    /// <summary>The divide key.</summary>
    Divide = Decimal | LButton, // 0x0000006F
    /// <summary>The F1 key.</summary>
    F1 = NumPad0 | ShiftKey, // 0x00000070
    /// <summary>The F2 key.</summary>
    F2 = F1 | LButton, // 0x00000071
    /// <summary>The F3 key.</summary>
    F3 = F1 | RButton, // 0x00000072
    /// <summary>The F4 key.</summary>
    F4 = F3 | LButton, // 0x00000073
    /// <summary>The F5 key.</summary>
    F5 = F1 | MButton, // 0x00000074
    /// <summary>The F6 key.</summary>
    F6 = F5 | LButton, // 0x00000075
    /// <summary>The F7 key.</summary>
    F7 = F5 | RButton, // 0x00000076
    /// <summary>The F8 key.</summary>
    F8 = F7 | LButton, // 0x00000077
    /// <summary>The F9 key.</summary>
    F9 = F1 | Back, // 0x00000078
    /// <summary>The F10 key.</summary>
    F10 = F9 | LButton, // 0x00000079
    /// <summary>The F11 key.</summary>
    F11 = F9 | RButton, // 0x0000007A
    /// <summary>The F12 key.</summary>
    F12 = F11 | LButton, // 0x0000007B
    /// <summary>The F13 key.</summary>
    F13 = F9 | MButton, // 0x0000007C
    /// <summary>The F14 key.</summary>
    F14 = F13 | LButton, // 0x0000007D
    /// <summary>The F15 key.</summary>
    F15 = F13 | RButton, // 0x0000007E
    /// <summary>The F16 key.</summary>
    F16 = F15 | LButton, // 0x0000007F
    /// <summary>The F17 key.</summary>
    F17 = 128, // 0x00000080
    /// <summary>The F18 key.</summary>
    F18 = F17 | LButton, // 0x00000081
    /// <summary>The F19 key.</summary>
    F19 = F17 | RButton, // 0x00000082
    /// <summary>The F20 key.</summary>
    F20 = F19 | LButton, // 0x00000083
    /// <summary>The F21 key.</summary>
    F21 = F17 | MButton, // 0x00000084
    /// <summary>The F22 key.</summary>
    F22 = F21 | LButton, // 0x00000085
    /// <summary>The F23 key.</summary>
    F23 = F21 | RButton, // 0x00000086
    /// <summary>The F24 key.</summary>
    F24 = F23 | LButton, // 0x00000087

    //
    // 0x88 - 0x8F : Unassigned
    //

    /// <summary>The NUM LOCK key.</summary>
    NumLock = F17 | ShiftKey, // 0x00000090
    /// <summary>The SCROLL LOCK key.</summary>
    Scroll = NumLock | LButton, // 0x00000091


    // 0x92 - 0x96 : OEM Specific

    // 0x97 - 0x9F : Unassigned

    //
    // L* & R* - left and right Alt, Ctrl and Shift virtual keys.
    // Used only as parameters to GetAsyncKeyState() and GetKeyState().
    // No other API or message will distinguish left and right keys in this way.
    //

    /// <summary>The left SHIFT key.</summary>
    [Key("LShift")]
    LShiftKey = F17 | Space, // 0x000000A0
    /// <summary>The right SHIFT key.</summary>
    [Key("RShift")]
    RShiftKey = LShiftKey | LButton, // 0x000000A1
    /// <summary>The left CTRL key.</summary>
    [Key("LCtrl")]
    LControlKey = LShiftKey | RButton, // 0x000000A2
    /// <summary>The right CTRL key.</summary>
    [Key("RCtrl")]
    RControlKey = LControlKey | LButton, // 0x000000A3
    /// <summary>The left ALT key.</summary>
    [Key("LAlt")]
    LMenu = LShiftKey | MButton, // 0x000000A4
    /// <summary>The right ALT key.</summary>
    [Key("RAlt")]
    RMenu = LMenu | LButton, // 0x000000A5
    /// <summary>The browser back key.</summary>
    BrowserBack = LMenu | RButton, // 0x000000A6
    /// <summary>The browser forward key.</summary>
    BrowserForward = BrowserBack | LButton, // 0x000000A7
    /// <summary>The browser refresh key.</summary>
    BrowserRefresh = LShiftKey | Back, // 0x000000A8
    /// <summary>The browser stop key.</summary>
    BrowserStop = BrowserRefresh | LButton, // 0x000000A9
    /// <summary>The browser search key.</summary>
    BrowserSearch = BrowserRefresh | RButton, // 0x000000AA
    /// <summary>The browser favorites key.</summary>
    BrowserFavorites = BrowserSearch | LButton, // 0x000000AB
    /// <summary>The browser home key.</summary>
    BrowserHome = BrowserRefresh | MButton, // 0x000000AC
    /// <summary>The volume mute key.</summary>
    VolumeMute = BrowserHome | LButton, // 0x000000AD
    /// <summary>The volume down key.</summary>
    VolumeDown = BrowserHome | RButton, // 0x000000AE
    /// <summary>The volume up key.</summary>
    VolumeUp = VolumeDown | LButton, // 0x000000AF
    /// <summary>The media next track key.</summary>
    MediaNextTrack = LShiftKey | ShiftKey, // 0x000000B0
    /// <summary>The media previous track key.</summary>
    MediaPreviousTrack = MediaNextTrack | LButton, // 0x000000B1
    /// <summary>The media Stop key.</summary>
    MediaStop = MediaNextTrack | RButton, // 0x000000B2
    /// <summary>The media play pause key.</summary>
    MediaPlayPause = MediaStop | LButton, // 0x000000B3
    /// <summary>The launch mail key.</summary>
    LaunchMail = MediaNextTrack | MButton, // 0x000000B4
    /// <summary>The select media key.</summary>
    SelectMedia = LaunchMail | LButton, // 0x000000B5
    /// <summary>The start application one key.</summary>
    LaunchApplication1 = LaunchMail | RButton, // 0x000000B6
    /// <summary>The start application two key.</summary>
    LaunchApplication2 = LaunchApplication1 | LButton, // 0x000000B7

    //
    // 0xB8 - 0xB9 : Reserved
    //

    /// <summary>The OEM Semicolon key on a US standard keyboard.</summary>
    [Key(";")]
    OemSemicolon = MediaStop | Back, // 0x000000BA
    /// <summary>The OEM 1 key.</summary>
    Oem1 = OemSemicolon, // 0x000000BA
    /// <summary>The OEM plus key on any country/region keyboard.</summary>
    [Key("=")]
    Oemplus = Oem1 | LButton, // 0x000000BB
    /// <summary>The OEM comma key on any country/region keyboard.</summary>
    [Key(",")]
    Oemcomma = LaunchMail | Back, // 0x000000BC
    /// <summary>The OEM minus key on any country/region keyboard.</summary>
    [Key("-")]
    OemMinus = Oemcomma | LButton, // 0x000000BD
    /// <summary>The OEM period key on any country/region keyboard.</summary>

    [Key(".")]
    OemPeriod = Oemcomma | RButton, // 0x000000BE
    /// <summary>The OEM question mark key on a US standard keyboard.</summary>
    [Key("/")]
    OemQuestion = OemPeriod | LButton, // 0x000000BF
    /// <summary>The OEM 2 key.  Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '/?' key </summary>
    [Key("/")]
    Oem2 = OemQuestion, // 0x000000BF
    /// <summary>The OEM 3 key.  Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '`~' key </summary>
    Oem3 = 192, // 0x000000C0
    /// <summary>The OEM tilde key on a US standard keyboard.</summary>

    [Key("`")]
    Oemtilde = Oem3, // 0x000000C0

    //
    // 0xC1 - 0xD7 : Reserved
    //

    //
    // 0xD8 - 0xDA : Unassigned
    //

    /// <summary>The OEM open bracket key on a US standard keyboard.</summary>
    [Key("[")]
    OemOpenBrackets = Oemtilde | Escape, // 0x000000DB
    /// <summary>The OEM 4 key. Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '[{' key</summary>
    [Key("[")]
    Oem4 = OemOpenBrackets, // 0x000000DB
    /// <summary>The OEM pipe key on a US standard keyboard. on US keyboard the Pipe is the the '\' key </summary>
    [Key(@"\")]
    OemPipe = Oemtilde | IMEConvert, // 0x000000DC
    /// <summary>The OEM 5 key. on US keyboard the Pipe is the the '\' key. Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '\|' key</summary>
    [Key(@"\")]
    Oem5 = OemPipe, // 0x000000DC
    /// <summary>The OEM close bracket key on a US standard keyboard.</summary>
    [Key("]")]
    OemCloseBrackets = Oem5 | LButton, // 0x000000DD
    /// <summary>The OEM 6 key. Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the ']}' key</summary>
    [Key("]")]
    Oem6 = OemCloseBrackets, // 0x000000DD
    /// <summary>The OEM 7 key.
    /// </summary>
    Oem7 = Oem5 | RButton, // 0x000000DE
    /// <summary>The OEM singled/double quote key on a US standard keyboard. Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the 'single-quote/double-quote' key</summary>
    [Key("'")]
    OemQuotes = Oem7, // 0x000000DE
    /// <summary>The OEM 8 key.Used for miscellaneous characters; it can vary by keyboard.</summary>
    [Key("'")]
    Oem8 = OemQuotes | LButton, // 0x000000DF

    //
    // 0xE0 : Reserved
    //

    //
    // 0xE1 : OEM Specific
    //

    /// <summary>
    /// The OEM 102 key.
    /// Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
    /// </summary>
    // [Key(@"\")]
    Oem102 = Oemtilde | PageDown, // 0x000000E2
    /// <summary>The OEM angle bracket or backslash key on the RT 102 key keyboard.</summary>
    // [Key(@"\")]
    OemBackslash = Oem102, // 0x000000E2

    //
    // (0xE3-E4) : OEM specific
    //

    /// <summary>The PROCESS KEY key.</summary>
    ProcessKey = Oemtilde | Left, // 0x000000E5

    //
    // 0xE6 : OEM specific
    //

    /// <summary>
    /// Used to pass Unicode characters as if they were keystrokes. The Packet key value is the low word of a 32-bit virtual-key value used for non-keyboard input methods.
    /// Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes. The PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods.
    /// For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
    /// </summary>
    Packet = ProcessKey | RButton, // 0x000000E7
    //
    // 0xE8 : Unassigned
    //

    //
    // 0xE9-F5 : OEM specific
    //
    /// <summary>The ATTN key.</summary>
    Attn = OemBackslash | CapsLock, // 0x000000F6
    /// <summary>The CRSEL key.</summary>
    Crsel = Attn | LButton, // 0x000000F7
    /// <summary>The EXSEL key.</summary>
    Exsel = Oemtilde | D8, // 0x000000F8
    /// <summary>The ERASE EOF key.</summary>
    EraseEof = Exsel | LButton, // 0x000000F9
    /// <summary>The PLAY key.</summary>
    Play = Exsel | RButton, // 0x000000FA
    /// <summary>The ZOOM key.</summary>
    Zoom = Play | LButton, // 0x000000FB
    /// <summary>A constant reserved for future use.</summary>
    NoName = Exsel | MButton, // 0x000000FC
    /// <summary>The PA1 key.</summary>
    Pa1 = NoName | LButton, // 0x000000FD
    /// <summary>The CLEAR key.</summary>
    OemClear = NoName | RButton, // 0x000000FE
    /// <summary>The SHIFT modifier key.</summary>
    Shift = 65536, // 0x00010000
    /// <summary>The CTRL modifier key.</summary>
    [Key("Ctrl")]
    Control = 131072, // 0x00020000
    /// <summary>The ALT modifier key.</summary>
    Alt = 262144, // 0x00040000
}

public static class KeyCodesEnumExtensions
{
    public static string KeyName(this KeyCodes value)
    {
        var name = Enum.GetName(value);
        var field = value.GetType().GetField(name);
        var attribute = field?.GetCustomAttribute<KeyAttribute>();
        return attribute?.KeyLetter ?? name;
    }
    public static bool IsLetterKey(this KeyCodes value)
    {
        return (value >= KeyCodes.A && value <= KeyCodes.Z) || (value >= KeyCodes.D0 && value <= KeyCodes.D9) || (value >= KeyCodes.NumPad0 && value <= KeyCodes.NumPad9);
    }
    public static bool IsAToZKey(this KeyCodes value)
    {
        return (value >= KeyCodes.A && value <= KeyCodes.Z);
    }
    public static bool IsShiftKey(this KeyCodes value)
    {
        return value == KeyCodes.ShiftKey || value == KeyCodes.LShiftKey || value == KeyCodes.RShiftKey;
    }
}