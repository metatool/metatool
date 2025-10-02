using System.Collections.Generic;

namespace Metatool.Service.MouseKey;

public partial class Combination
{
    public Combination(char key): this(CharToKeyMap[key]) { }

    private Combination((KeyCodes trigger, KeyCodes[] chord) keyTuple):this(keyTuple.trigger, keyTuple.chord){}

    private static readonly Dictionary<char, (KeyCodes trigger, KeyCodes[] chord)> CharToKeyMap = new ()
    {
        // Letters (lowercase - no modifiers needed)
        { 'a', (KeyCodes.A,[])}, { 'b', (KeyCodes.B,[])}, { 'c', (KeyCodes.C,[])}, { 'd', (KeyCodes.D,[])},
        { 'e', (KeyCodes.E,[])}, { 'f', (KeyCodes.F,[])}, { 'g', (KeyCodes.G,[])}, { 'h', (KeyCodes.H,[])},
        { 'i', (KeyCodes.I,[])}, { 'j', (KeyCodes.J,[])}, { 'k', (KeyCodes.K,[])}, { 'l', (KeyCodes.L,[])},
        { 'm', (KeyCodes.M,[])}, { 'n', (KeyCodes.N,[])}, { 'o', (KeyCodes.O,[])}, { 'p', (KeyCodes.P,[])},
        { 'q', (KeyCodes.Q,[])}, { 'r', (KeyCodes.R,[])}, { 's', (KeyCodes.S,[])}, { 't', (KeyCodes.T,[])},
        { 'u', (KeyCodes.U,[])}, { 'v', (KeyCodes.V,[])}, { 'w', (KeyCodes.W,[])}, { 'x', (KeyCodes.X,[])},
        { 'y', (KeyCodes.Y,[])}, { 'z', (KeyCodes.Z,[])},

        // Letters (uppercase - shift required)
        { 'A', (KeyCodes.A, [KeyCodes.Shift])}, { 'B', (KeyCodes.B, [KeyCodes.Shift])},
        { 'C', (KeyCodes.C, [KeyCodes.Shift])},
        { 'D', (KeyCodes.D, [KeyCodes.Shift])}, { 'E', (KeyCodes.E, [KeyCodes.Shift])},
        { 'F', (KeyCodes.F, [KeyCodes.Shift])},
        { 'G', (KeyCodes.G, [KeyCodes.Shift])}, { 'H', (KeyCodes.H, [KeyCodes.Shift])},
        { 'I', (KeyCodes.I, [KeyCodes.Shift])},
        { 'J', (KeyCodes.J, [KeyCodes.Shift])}, { 'K', (KeyCodes.K, [KeyCodes.Shift])},
        { 'L', (KeyCodes.L, [KeyCodes.Shift])},
        { 'M', (KeyCodes.M, [KeyCodes.Shift])}, { 'N', (KeyCodes.N, [KeyCodes.Shift])},
        { 'O', (KeyCodes.O, [KeyCodes.Shift])},
        { 'P', (KeyCodes.P, [KeyCodes.Shift])}, { 'Q', (KeyCodes.Q, [KeyCodes.Shift])},
        { 'R', (KeyCodes.R, [KeyCodes.Shift])},
        { 'S', (KeyCodes.S, [KeyCodes.Shift])}, { 'T', (KeyCodes.T, [KeyCodes.Shift])},
        { 'U', (KeyCodes.U, [KeyCodes.Shift])},
        { 'V', (KeyCodes.V, [KeyCodes.Shift])}, { 'W', (KeyCodes.W, [KeyCodes.Shift])},
        { 'X', (KeyCodes.X, [KeyCodes.Shift])},
        { 'Y', (KeyCodes.Y, [KeyCodes.Shift])}, { 'Z', (KeyCodes.Z, [KeyCodes.Shift])},

        // Numbers (no modifiers needed)
        { '0', (KeyCodes.D0,[])}, { '1', (KeyCodes.D1,[])}, { '2', (KeyCodes.D2,[])}, { '3', (KeyCodes.D3,[])},
        { '4', (KeyCodes.D4,[])}, { '5', (KeyCodes.D5,[])}, { '6', (KeyCodes.D6,[])}, { '7', (KeyCodes.D7,[])},
        { '8', (KeyCodes.D8,[])}, { '9', (KeyCodes.D9,[])},

        // Symbols that require Shift
        { '!', (KeyCodes.D1, [KeyCodes.Shift])}, // Shift + 1
        { '@', (KeyCodes.D2, [KeyCodes.Shift])}, // Shift + 2
        { '#', (KeyCodes.D3, [KeyCodes.Shift])}, // Shift + 3
        { '$', (KeyCodes.D4, [KeyCodes.Shift])}, // Shift + 4
        { '%', (KeyCodes.D5, [KeyCodes.Shift])}, // Shift + 5
        { '^', (KeyCodes.D6, [KeyCodes.Shift])}, // Shift + 6
        { '&', (KeyCodes.D7, [KeyCodes.Shift])}, // Shift + 7
        { '*', (KeyCodes.D8, [KeyCodes.Shift])}, // Shift + 8
        { '(', (KeyCodes.D9, [KeyCodes.Shift])}, // Shift + 9
        { ')', (KeyCodes.D0, [KeyCodes.Shift])}, // Shift + 0

        { '_', (KeyCodes.OemMinus, [KeyCodes.Shift])}, // Shift + -
        { '+', (KeyCodes.Oemplus, [KeyCodes.Shift])}, // Shift + =
        { '|', (KeyCodes.OemBackslash, [KeyCodes.Shift])}, // Shift + \
        { '~', (KeyCodes.Oemtilde, [KeyCodes.Shift])}, // Shift + `
        { '{', (KeyCodes.OemOpenBrackets, [KeyCodes.Shift])}, // Shift + [
        { '}', (KeyCodes.OemCloseBrackets, [KeyCodes.Shift])}, // Shift + ]
        { ':', (KeyCodes.OemSemicolon, [KeyCodes.Shift])}, // Shift + ;
        { '"', (KeyCodes.OemQuotes, [KeyCodes.Shift])}, // Shift + '
        { '<', (KeyCodes.Oemcomma, [KeyCodes.Shift])}, // Shift + ,
        { '>', (KeyCodes.OemPeriod, [KeyCodes.Shift])}, // Shift + .
        { '?', (KeyCodes.OemQuestion, [KeyCodes.Shift])}, // Shift + /

        // Symbols that don't require modifiers
        { '-', (KeyCodes.OemMinus,[])},
        { '=', (KeyCodes.Oemplus,[])},
        { '\\', (KeyCodes.OemBackslash,[])},
        { '`', (KeyCodes.Oemtilde,[])},
        { '[', (KeyCodes.OemOpenBrackets,[])},
        { ']', (KeyCodes.OemCloseBrackets,[])},
        { ';', (KeyCodes.OemSemicolon,[])},
        { '\'', (KeyCodes.OemQuotes,[])},
        { ',', (KeyCodes.Oemcomma,[])},
        { '.', (KeyCodes.OemPeriod,[])},
        { '/', (KeyCodes.OemQuestion,[])},

        // Special keys
        { ' ', (KeyCodes.Space,[])},
        { '\t', (KeyCodes.Tab,[])},
        { '\r', (KeyCodes.Return,[])},
        { '\n', (KeyCodes.Return,[])},
        { '\b', (KeyCodes.Back,[])},
        { '\x1b', (KeyCodes.Escape,[])}, // ESC character

        // Control characters (Ctrl combinations)
        { '\x01', (KeyCodes.A, [KeyCodes.Control])}, // Ctrl+A (SOH)
        { '\x02', (KeyCodes.B, [KeyCodes.Control])}, // Ctrl+B (STX)
        { '\x03', (KeyCodes.C, [KeyCodes.Control])}, // Ctrl+C (ETX)
        { '\x04', (KeyCodes.D, [KeyCodes.Control])}, // Ctrl+D (EOT)
        { '\x05', (KeyCodes.E, [KeyCodes.Control])}, // Ctrl+E (ENQ)
        { '\x06', (KeyCodes.F, [KeyCodes.Control])}, // Ctrl+F (ACK)
        { '\x07', (KeyCodes.G, [KeyCodes.Control])}, // Ctrl+G (BEL)
        // { '\x08', (KeyValues.H, [KeyValues.Control])}, // Ctrl+H (BS) - same as backspace \b
        // '\x09' is Tab, handled above
        // { '\x0A', (KeyValues.J, [KeyValues.Control])}, // Ctrl+J (LF)
        { '\x0B', (KeyCodes.K, [KeyCodes.Control])}, // Ctrl+K (VT)
        { '\x0C', (KeyCodes.L, [KeyCodes.Control])}, // Ctrl+L (FF)
        // '\x0D' is Enter, handled above
        { '\x0E', (KeyCodes.N, [KeyCodes.Control])}, // Ctrl+N (SO)
        { '\x0F', (KeyCodes.O, [KeyCodes.Control])}, // Ctrl+O (SI)
        { '\x10', (KeyCodes.P, [KeyCodes.Control])}, // Ctrl+P (DLE)
        { '\x11', (KeyCodes.Q, [KeyCodes.Control])}, // Ctrl+Q (DC1)
        { '\x12', (KeyCodes.R, [KeyCodes.Control])}, // Ctrl+R (DC2)
        { '\x13', (KeyCodes.S, [KeyCodes.Control])}, // Ctrl+S (DC3)
        { '\x14', (KeyCodes.T, [KeyCodes.Control])}, // Ctrl+T (DC4)
        { '\x15', (KeyCodes.U, [KeyCodes.Control])}, // Ctrl+U (NAK)
        { '\x16', (KeyCodes.V, [KeyCodes.Control])}, // Ctrl+V (SYN)
        { '\x17', (KeyCodes.W, [KeyCodes.Control])}, // Ctrl+W (ETB)
        { '\x18', (KeyCodes.X, [KeyCodes.Control])}, // Ctrl+X (CAN)
        { '\x19', (KeyCodes.Y, [KeyCodes.Control])}, // Ctrl+Y (EM)
        { '\x1A', (KeyCodes.Z, [KeyCodes.Control])}, // Ctrl+Z (SUB)
        // '\x1B' is Escape, handled above
        { '\x1C', (KeyCodes.OemBackslash, [KeyCodes.Control])}, // Ctrl+\ (FS)
        { '\x1D', (KeyCodes.OemCloseBrackets, [KeyCodes.Control])}, // Ctrl+] (GS)
        { '\x1E', (KeyCodes.D6, [KeyCodes.Shift, KeyCodes.Control])}, // Ctrl+^ (RS)
        { '\x1F', (KeyCodes.OemMinus, [KeyCodes.Shift, KeyCodes.Control])}, // Ctrl+_ (US)
    };
}