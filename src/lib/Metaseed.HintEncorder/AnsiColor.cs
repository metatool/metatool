namespace Metaseed.HintEncorder;

public enum ForegroundColor
{
    Black = 30,
    Red = 31,
    Green = 32,
    Yellow = 33,
    Blue = 34,
    Magenta = 35,
    Cyan = 36,
    LightGray = 37,

    DarkGray = 90,
    LightRed = 91,
    LightGreen = 92,
    LightYellow = 93,
    LightBlue = 94,
    LightMagenta = 95,
    LightCyan = 96,
    White = 97
}

public enum BackgroundColor
{
    Black = 40,
    Red = 41,
    Green = 42,
    Yellow = 43,
    Blue = 44,
    Magenta = 45,
    Cyan = 46,
    LightGray = 47,

    DarkGray = 100,
    LightRed = 101,
    LightGreen = 102,
    LightYellow = 103,
    LightBlue = 104,
    LightMagenta = 105,
    LightCyan = 106,
    White = 107
}

public static class AnsiColor
{
    private const char Esc = '\x1b';
    public static readonly string Reset = $"{Esc}[0m";

    public static int GetAnsiCode(string name, bool isBackground)
    {
        if (isBackground)
            return (int)Enum.Parse<BackgroundColor>(name, ignoreCase: true);
        return (int)Enum.Parse<ForegroundColor>(name, ignoreCase: true);
    }

    public static string Underline(string text) => $"{Esc}[4m{text}{Reset}";
    public static string Italic(string text) => $"{Esc}[3m{text}{Reset}";
    public static string ClearToEndOfLine => $"{Esc}[K";

    public static string WithBackground(string text, string bgColorName)
    {
        var bgCode = GetAnsiCode(bgColorName, isBackground: true);
        return $"{Esc}[{bgCode}m{Esc}[30m{text}{Reset}";
    }
}
