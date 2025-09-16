using System.Collections.Generic;

namespace Metatool.Service.MouseKey;

public partial class Combination
{
    public Combination(char key): this(CharToKeyMap[key]) { }

    private Combination((KeyValues trigger, KeyValues[] chord) keyTuple):this(keyTuple.trigger, keyTuple.chord){}

    private static readonly Dictionary<char, (KeyValues trigger, KeyValues[] chord)> CharToKeyMap = new ()
    {
        // Letters (lowercase - no modifiers needed)
        { 'a', (KeyValues.A,[])}, { 'b', (KeyValues.B,[])}, { 'c', (KeyValues.C,[])}, { 'd', (KeyValues.D,[])},
        { 'e', (KeyValues.E,[])}, { 'f', (KeyValues.F,[])}, { 'g', (KeyValues.G,[])}, { 'h', (KeyValues.H,[])},
        { 'i', (KeyValues.I,[])}, { 'j', (KeyValues.J,[])}, { 'k', (KeyValues.K,[])}, { 'l', (KeyValues.L,[])},
        { 'm', (KeyValues.M,[])}, { 'n', (KeyValues.N,[])}, { 'o', (KeyValues.O,[])}, { 'p', (KeyValues.P,[])},
        { 'q', (KeyValues.Q,[])}, { 'r', (KeyValues.R,[])}, { 's', (KeyValues.S,[])}, { 't', (KeyValues.T,[])},
        { 'u', (KeyValues.U,[])}, { 'v', (KeyValues.V,[])}, { 'w', (KeyValues.W,[])}, { 'x', (KeyValues.X,[])},
        { 'y', (KeyValues.Y,[])}, { 'z', (KeyValues.Z,[])},

        // Letters (uppercase - shift required)
        { 'A', (KeyValues.A, [KeyValues.Shift])}, { 'B', (KeyValues.B, [KeyValues.Shift])},
        { 'C', (KeyValues.C, [KeyValues.Shift])},
        { 'D', (KeyValues.D, [KeyValues.Shift])}, { 'E', (KeyValues.E, [KeyValues.Shift])},
        { 'F', (KeyValues.F, [KeyValues.Shift])},
        { 'G', (KeyValues.G, [KeyValues.Shift])}, { 'H', (KeyValues.H, [KeyValues.Shift])},
        { 'I', (KeyValues.I, [KeyValues.Shift])},
        { 'J', (KeyValues.J, [KeyValues.Shift])}, { 'K', (KeyValues.K, [KeyValues.Shift])},
        { 'L', (KeyValues.L, [KeyValues.Shift])},
        { 'M', (KeyValues.M, [KeyValues.Shift])}, { 'N', (KeyValues.N, [KeyValues.Shift])},
        { 'O', (KeyValues.O, [KeyValues.Shift])},
        { 'P', (KeyValues.P, [KeyValues.Shift])}, { 'Q', (KeyValues.Q, [KeyValues.Shift])},
        { 'R', (KeyValues.R, [KeyValues.Shift])},
        { 'S', (KeyValues.S, [KeyValues.Shift])}, { 'T', (KeyValues.T, [KeyValues.Shift])},
        { 'U', (KeyValues.U, [KeyValues.Shift])},
        { 'V', (KeyValues.V, [KeyValues.Shift])}, { 'W', (KeyValues.W, [KeyValues.Shift])},
        { 'X', (KeyValues.X, [KeyValues.Shift])},
        { 'Y', (KeyValues.Y, [KeyValues.Shift])}, { 'Z', (KeyValues.Z, [KeyValues.Shift])},

        // Numbers (no modifiers needed)
        { '0', (KeyValues.D0,[])}, { '1', (KeyValues.D1,[])}, { '2', (KeyValues.D2,[])}, { '3', (KeyValues.D3,[])},
        { '4', (KeyValues.D4,[])}, { '5', (KeyValues.D5,[])}, { '6', (KeyValues.D6,[])}, { '7', (KeyValues.D7,[])},
        { '8', (KeyValues.D8,[])}, { '9', (KeyValues.D9,[])},

        // Symbols that require Shift
        { '!', (KeyValues.D1, [KeyValues.Shift])}, // Shift + 1
        { '@', (KeyValues.D2, [KeyValues.Shift])}, // Shift + 2
        { '#', (KeyValues.D3, [KeyValues.Shift])}, // Shift + 3
        { '$', (KeyValues.D4, [KeyValues.Shift])}, // Shift + 4
        { '%', (KeyValues.D5, [KeyValues.Shift])}, // Shift + 5
        { '^', (KeyValues.D6, [KeyValues.Shift])}, // Shift + 6
        { '&', (KeyValues.D7, [KeyValues.Shift])}, // Shift + 7
        { '*', (KeyValues.D8, [KeyValues.Shift])}, // Shift + 8
        { '(', (KeyValues.D9, [KeyValues.Shift])}, // Shift + 9
        { ')', (KeyValues.D0, [KeyValues.Shift])}, // Shift + 0

        { '_', (KeyValues.OemMinus, [KeyValues.Shift])}, // Shift + -
        { '+', (KeyValues.Oemplus, [KeyValues.Shift])}, // Shift + =
        { '|', (KeyValues.OemBackslash, [KeyValues.Shift])}, // Shift + \
        { '~', (KeyValues.Oemtilde, [KeyValues.Shift])}, // Shift + `
        { '{', (KeyValues.OemOpenBrackets, [KeyValues.Shift])}, // Shift + [
        { '}', (KeyValues.OemCloseBrackets, [KeyValues.Shift])}, // Shift + ]
        { ':', (KeyValues.OemSemicolon, [KeyValues.Shift])}, // Shift + ;
        { '"', (KeyValues.OemQuotes, [KeyValues.Shift])}, // Shift + '
        { '<', (KeyValues.Oemcomma, [KeyValues.Shift])}, // Shift + ,
        { '>', (KeyValues.OemPeriod, [KeyValues.Shift])}, // Shift + .
        { '?', (KeyValues.OemQuestion, [KeyValues.Shift])}, // Shift + /

        // Symbols that don't require modifiers
        { '-', (KeyValues.OemMinus,[])},
        { '=', (KeyValues.Oemplus,[])},
        { '\\', (KeyValues.OemBackslash,[])},
        { '`', (KeyValues.Oemtilde,[])},
        { '[', (KeyValues.OemOpenBrackets,[])},
        { ']', (KeyValues.OemCloseBrackets,[])},
        { ';', (KeyValues.OemSemicolon,[])},
        { '\'', (KeyValues.OemQuotes,[])},
        { ',', (KeyValues.Oemcomma,[])},
        { '.', (KeyValues.OemPeriod,[])},
        { '/', (KeyValues.OemQuestion,[])},

        // Special keys
        { ' ', (KeyValues.Space,[])},
        { '\t', (KeyValues.Tab,[])},
        { '\r', (KeyValues.Return,[])},
        { '\n', (KeyValues.Return,[])},
        { '\b', (KeyValues.Back,[])},
        { '\x1b', (KeyValues.Escape,[])}, // ESC character

        // Control characters (Ctrl combinations)
        { '\x01', (KeyValues.A, [KeyValues.Control])}, // Ctrl+A (SOH)
        { '\x02', (KeyValues.B, [KeyValues.Control])}, // Ctrl+B (STX)
        { '\x03', (KeyValues.C, [KeyValues.Control])}, // Ctrl+C (ETX)
        { '\x04', (KeyValues.D, [KeyValues.Control])}, // Ctrl+D (EOT)
        { '\x05', (KeyValues.E, [KeyValues.Control])}, // Ctrl+E (ENQ)
        { '\x06', (KeyValues.F, [KeyValues.Control])}, // Ctrl+F (ACK)
        { '\x07', (KeyValues.G, [KeyValues.Control])}, // Ctrl+G (BEL)
        { '\x08', (KeyValues.H, [KeyValues.Control])}, // Ctrl+H (BS) - same as backspace
        // '\x09' is Tab, handled above
        { '\x0A', (KeyValues.J, [KeyValues.Control])}, // Ctrl+J (LF)
        { '\x0B', (KeyValues.K, [KeyValues.Control])}, // Ctrl+K (VT)
        { '\x0C', (KeyValues.L, [KeyValues.Control])}, // Ctrl+L (FF)
        // '\x0D' is Enter, handled above
        { '\x0E', (KeyValues.N, [KeyValues.Control])}, // Ctrl+N (SO)
        { '\x0F', (KeyValues.O, [KeyValues.Control])}, // Ctrl+O (SI)
        { '\x10', (KeyValues.P, [KeyValues.Control])}, // Ctrl+P (DLE)
        { '\x11', (KeyValues.Q, [KeyValues.Control])}, // Ctrl+Q (DC1)
        { '\x12', (KeyValues.R, [KeyValues.Control])}, // Ctrl+R (DC2)
        { '\x13', (KeyValues.S, [KeyValues.Control])}, // Ctrl+S (DC3)
        { '\x14', (KeyValues.T, [KeyValues.Control])}, // Ctrl+T (DC4)
        { '\x15', (KeyValues.U, [KeyValues.Control])}, // Ctrl+U (NAK)
        { '\x16', (KeyValues.V, [KeyValues.Control])}, // Ctrl+V (SYN)
        { '\x17', (KeyValues.W, [KeyValues.Control])}, // Ctrl+W (ETB)
        { '\x18', (KeyValues.X, [KeyValues.Control])}, // Ctrl+X (CAN)
        { '\x19', (KeyValues.Y, [KeyValues.Control])}, // Ctrl+Y (EM)
        { '\x1A', (KeyValues.Z, [KeyValues.Control])}, // Ctrl+Z (SUB)
        // '\x1B' is Escape, handled above
        { '\x1C', (KeyValues.OemBackslash, [KeyValues.Control])}, // Ctrl+\ (FS)
        { '\x1D', (KeyValues.OemCloseBrackets, [KeyValues.Control])}, // Ctrl+] (GS)
        { '\x1E', (KeyValues.D6, [KeyValues.Shift, KeyValues.Control])}, // Ctrl+^ (RS)
        { '\x1F', (KeyValues.OemMinus, [KeyValues.Shift, KeyValues.Control])}, // Ctrl+_ (US)
    };
}