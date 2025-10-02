using System;

namespace Metatool.Service.MouseKey;
// from System.Windows.Forms.Keys
[Flags]
public enum KeyCodes
{
    /// <summary>The bitmask to extract a key code from a key value.</summary>
    KeyCode = 65535, // 0x0000FFFF
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
    ShiftKey = 16, // 0x00000010
    /// <summary>The CTRL key.</summary>
    ControlKey = ShiftKey | LButton, // 0x00000011
    /// <summary>The ALT key.</summary>
    Menu = ShiftKey | RButton, // 0x00000012
    /// <summary>The PAUSE key.</summary>
    Pause = Menu | LButton, // 0x00000013
    /// <summary>The CAPS LOCK key.</summary>
    Capital = ShiftKey | MButton, // 0x00000014
    /// <summary>The CAPS LOCK key.</summary>
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
    Prior = Space | LButton, // 0x00000021
    /// <summary>The PAGE UP key.</summary>
    PageUp = Prior, // 0x00000021
    /// <summary>The PAGE DOWN key.</summary>
    Next = Space | RButton, // 0x00000022
    /// <summary>The PAGE DOWN key.</summary>
    PageDown = Next, // 0x00000022
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
    D0 = Space | ShiftKey, // 0x00000030
    /// <summary>The 1 key.</summary>
    D1 = D0 | LButton, // 0x00000031
    /// <summary>The 2 key.</summary>
    D2 = D0 | RButton, // 0x00000032
    /// <summary>The 3 key.</summary>
    D3 = D2 | LButton, // 0x00000033
    /// <summary>The 4 key.</summary>
    D4 = D0 | MButton, // 0x00000034
    /// <summary>The 5 key.</summary>
    D5 = D4 | LButton, // 0x00000035
    /// <summary>The 6 key.</summary>
    D6 = D4 | RButton, // 0x00000036
    /// <summary>The 7 key.</summary>
    D7 = D6 | LButton, // 0x00000037
    /// <summary>The 8 key.</summary>
    D8 = D0 | Back, // 0x00000038
    /// <summary>The 9 key.</summary>
    D9 = D8 | LButton, // 0x00000039
    /// <summary>The A key.</summary>
    A = 65, // 0x00000041
    /// <summary>The B key.</summary>
    B = 66, // 0x00000042
    /// <summary>The C key.</summary>
    C = B | LButton, // 0x00000043
    /// <summary>The D key.</summary>
    D = 68, // 0x00000044
    /// <summary>The E key.</summary>
    E = D | LButton, // 0x00000045
    /// <summary>The F key.</summary>
    F = D | RButton, // 0x00000046
    /// <summary>The G key.</summary>
    G = F | LButton, // 0x00000047
    /// <summary>The H key.</summary>
    H = 72, // 0x00000048
    /// <summary>The I key.</summary>
    I = H | LButton, // 0x00000049
    /// <summary>The J key.</summary>
    J = H | RButton, // 0x0000004A
    /// <summary>The K key.</summary>
    K = J | LButton, // 0x0000004B
    /// <summary>The L key.</summary>
    L = H | MButton, // 0x0000004C
    /// <summary>The M key.</summary>
    M = L | LButton, // 0x0000004D
    /// <summary>The N key.</summary>
    N = L | RButton, // 0x0000004E
    /// <summary>The O key.</summary>
    O = N | LButton, // 0x0000004F
    /// <summary>The P key.</summary>
    P = 80, // 0x00000050
    /// <summary>The Q key.</summary>
    Q = P | LButton, // 0x00000051
    /// <summary>The R key.</summary>
    R = P | RButton, // 0x00000052
    /// <summary>The S key.</summary>
    S = R | LButton, // 0x00000053
    /// <summary>The T key.</summary>
    T = P | MButton, // 0x00000054
    /// <summary>The U key.</summary>
    U = T | LButton, // 0x00000055
    /// <summary>The V key.</summary>
    V = T | RButton, // 0x00000056
    /// <summary>The W key.</summary>
    W = V | LButton, // 0x00000057
    /// <summary>The X key.</summary>
    X = P | Back, // 0x00000058
    /// <summary>The Y key.</summary>
    Y = X | LButton, // 0x00000059
    /// <summary>The Z key.</summary>
    Z = X | RButton, // 0x0000005A
    /// <summary>The left Windows logo key (Microsoft Natural Keyboard).</summary>
    LWin = Z | LButton, // 0x0000005B
    /// <summary>The right Windows logo key (Microsoft Natural Keyboard).</summary>
    RWin = X | MButton, // 0x0000005C
    /// <summary>The application key (Microsoft Natural Keyboard).</summary>
    Apps = RWin | LButton, // 0x0000005D
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
    /// <summary>The NUM LOCK key.</summary>
    NumLock = F17 | ShiftKey, // 0x00000090
    /// <summary>The SCROLL LOCK key.</summary>
    Scroll = NumLock | LButton, // 0x00000091
    /// <summary>The left SHIFT key.</summary>
    LShiftKey = F17 | Space, // 0x000000A0
    /// <summary>The right SHIFT key.</summary>
    RShiftKey = LShiftKey | LButton, // 0x000000A1
    /// <summary>The left CTRL key.</summary>
    LControlKey = LShiftKey | RButton, // 0x000000A2
    /// <summary>The right CTRL key.</summary>
    RControlKey = LControlKey | LButton, // 0x000000A3
    /// <summary>The left ALT key.</summary>
    LMenu = LShiftKey | MButton, // 0x000000A4
    /// <summary>The right ALT key.</summary>
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
    /// <summary>The OEM Semicolon key on a US standard keyboard.</summary>
    OemSemicolon = MediaStop | Back, // 0x000000BA
    /// <summary>The OEM 1 key.</summary>
    Oem1 = OemSemicolon, // 0x000000BA
    /// <summary>The OEM plus key on any country/region keyboard.</summary>
    Oemplus = Oem1 | LButton, // 0x000000BB
    /// <summary>The OEM comma key on any country/region keyboard.</summary>
    Oemcomma = LaunchMail | Back, // 0x000000BC
    /// <summary>The OEM minus key on any country/region keyboard.</summary>
    OemMinus = Oemcomma | LButton, // 0x000000BD
    /// <summary>The OEM period key on any country/region keyboard.</summary>
    OemPeriod = Oemcomma | RButton, // 0x000000BE
    /// <summary>The OEM question mark key on a US standard keyboard.</summary>
    OemQuestion = OemPeriod | LButton, // 0x000000BF
    /// <summary>The OEM 2 key.</summary>
    Oem2 = OemQuestion, // 0x000000BF
    /// <summary>The OEM 3 key.</summary>
    Oem3 = 192, // 0x000000C0
    /// <summary>The OEM tilde key on a US standard keyboard.</summary>
    Oemtilde = Oem3, // 0x000000C0
    /// <summary>The OEM open bracket key on a US standard keyboard.</summary>
    OemOpenBrackets = Oemtilde | Escape, // 0x000000DB
    /// <summary>The OEM 4 key.</summary>
    Oem4 = OemOpenBrackets, // 0x000000DB
    /// <summary>The OEM pipe key on a US standard keyboard.</summary>
    OemPipe = Oemtilde | IMEConvert, // 0x000000DC
    /// <summary>The OEM 5 key.</summary>
    Oem5 = OemPipe, // 0x000000DC
    /// <summary>The OEM close bracket key on a US standard keyboard.</summary>
    OemCloseBrackets = Oem5 | LButton, // 0x000000DD
    /// <summary>The OEM 6 key.</summary>
    Oem6 = OemCloseBrackets, // 0x000000DD
    /// <summary>The OEM 7 key.</summary>
    Oem7 = Oem5 | RButton, // 0x000000DE
    /// <summary>The OEM singled/double quote key on a US standard keyboard.</summary>
    OemQuotes = Oem7, // 0x000000DE
    /// <summary>The OEM 8 key.</summary>
    Oem8 = OemQuotes | LButton, // 0x000000DF
    /// <summary>The OEM 102 key.</summary>
    Oem102 = Oemtilde | PageDown, // 0x000000E2
    /// <summary>The OEM angle bracket or backslash key on the RT 102 key keyboard.</summary>
    OemBackslash = Oem102, // 0x000000E2
    /// <summary>The PROCESS KEY key.</summary>
    ProcessKey = Oemtilde | Left, // 0x000000E5
    /// <summary>Used to pass Unicode characters as if they were keystrokes. The Packet key value is the low word of a 32-bit virtual-key value used for non-keyboard input methods.</summary>
    Packet = ProcessKey | RButton, // 0x000000E7
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
    Control = 131072, // 0x00020000
    /// <summary>The ALT modifier key.</summary>
    Alt = 262144, // 0x00040000
}