namespace Metaseed.HintEncorder;

/// <summary>
/// Shows and restores a start indicator (emoji icon) near the cursor.
/// Equivalent to start-indicator.ps1.
/// </summary>
public static class ConsoleIndicator
{
    private static bool IsFirstCharOfLine(BufferInfo info)
    {
        return info.Cursor == 0 ||
               info.Line[..(info.Cursor)].LastIndexOf('\n') == info.Cursor - 1;
    }

    /// <summary>
    /// Draws an icon near the cursor position. Returns the icon length for later restoration.
    /// </summary>
    public static int ShowStartIndicator(BufferInfo info, string icon = "\U0001F3C3")
    {
        var len = icon.Length;
        var halfLen = len / 2;

        var drawLeft = IsFirstCharOfLine(info) ? info.ConsoleLeft : info.ConsoleLeft - halfLen;
        Console.SetCursorPosition(drawLeft, info.ConsoleTop);
        Console.Write(icon);
        Console.SetCursorPosition(info.ConsoleLeft, info.ConsoleTop);
        return icon.Length;
    }

    /// <summary>
    /// Restores the characters that were overwritten by the start indicator.
    /// </summary>
    public static void RestoreStartIndicator(BufferInfo info, int len = 2)
    {
        var drawLeft = info.ConsoleLeft;
        int restoreStart, restoreEnd;

        if (!IsFirstCharOfLine(info))
        {
            var halfLen = len / 2;
            drawLeft = info.ConsoleLeft - halfLen;
            restoreStart = info.Cursor - halfLen;
            restoreEnd = info.Cursor + len - halfLen;
        }
        else
        {
            restoreStart = info.Cursor;
            restoreEnd = info.Cursor + len;
        }

        // Clamp to valid range
        restoreStart = Math.Max(0, restoreStart);
        restoreEnd = Math.Min(info.Line.Length, restoreEnd);

        if (restoreStart < restoreEnd)
        {
            var restoreText = info.Line[restoreStart..restoreEnd];
            Console.SetCursorPosition(drawLeft, info.ConsoleTop);
            Console.Write(restoreText);
        }

        Console.SetCursorPosition(info.ConsoleLeft, info.ConsoleTop);
    }
}
