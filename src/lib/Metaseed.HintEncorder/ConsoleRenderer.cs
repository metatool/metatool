namespace Metaseed.HintEncorder;

/// <summary>
/// Draws visual overlays on the console: filter highlights, next-char hints, and jump codes.
/// Equivalent to visual.ps1 (Write-BufferText, Reset-View, Restore-Visuals, Draw-Overlay).
/// </summary>
public static class ConsoleRenderer
{
    /// <summary>
    /// Writes the buffer text back to the console at its original position,
    /// clearing to end-of-line for each line to remove stale overlay characters.
    /// </summary>
    public static void WriteBufferText(BufferInfo info)
    {
        var lines = info.Line.Replace("\r", "").Split('\n');
        var clearEol = AnsiColor.ClearToEndOfLine;

        for (var i = 0; i < lines.Length; i++)
        {
            if (i == 0)
            {
                if (info.StartTop >= 0)
                    Console.SetCursorPosition(info.StartLeft, info.StartTop);
            }
            else
            {
                var y = Console.CursorTop;
                if (Console.CursorLeft > 0 || lines[i - 1].Length == 0)
                    y++;
                if (y < Console.BufferHeight)
                    Console.SetCursorPosition(info.ContinuationPromptWidth, y);
            }
            Console.Write(lines[i] + clearEol);
        }
    }

    /// <summary>Restores the original buffer text on screen (clears previous overlays).</summary>
    public static void ResetView(BufferInfo info) => WriteBufferText(info);

    /// <summary>
    /// Clears overlays by rewriting original text, then forces the host to
    /// refresh syntax highlighting by inserting empty text.
    /// </summary>
    public static void RestoreVisuals(BufferInfo info, IBufferProvider provider)
    {
        var currentLeft = Console.CursorLeft;
        var currentTop = Console.CursorTop;

        WriteBufferText(info);

        Console.SetCursorPosition(currentLeft, currentTop);
        provider.Insert("");
    }

    /// <summary>
    /// Draws the complete overlay: underlined filter text, italic next-char hints,
    /// and colored jump codes on top.
    /// </summary>
    public static void DrawOverlay(BufferInfo info, int[] matches, string[] codes,
        int filterLength, MetaJumpConfig config, bool isRipple = true)
    {
        if (matches.Length == 0 || codes.Length == 0)
            return;

        ResetView(info);

        // 1. Draw filter text highlights and next-char hints
        foreach (var idx in matches)
        {
            var pos = GetConsolePosition(info, idx);

            // Draw filtered text with underline
            if (filterLength > 0)
            {
                var txt = info.Line.Substring(idx, filterLength);
                Console.SetCursorPosition(pos.X, pos.Y);
                Console.Write(AnsiColor.Underline(txt));
            }

            if (isRipple)
            {
                // Draw next char with italics
                var nextIdx = idx + filterLength;
                if (nextIdx < info.Line.Length)
                {
                    var nextPos = GetConsolePosition(info, nextIdx);
                    var nextChar = info.Line[nextIdx];
                    Console.SetCursorPosition(nextPos.X, nextPos.Y);
                    Console.Write(AnsiColor.Italic(nextChar.ToString()));
                }
            }
        }

        // 2. Draw codes on top
        for (var i = 0; i < matches.Length; i++)
        {
            var idx = matches[i];
            var code = codes[i];
            var pos = GetConsolePosition(info, idx);
            var bgName = config.GetBackgroundColor(code.Length);

            Console.SetCursorPosition(pos.X, pos.Y);
            Console.Write(AnsiColor.WithBackground(code, bgName));
        }
    }

    private static (int X, int Y) GetConsolePosition(BufferInfo info, int index)
    {
        var offset = BufferInfo.GetVisualOffset(
            info.Line, index, info.StartLeft, info.ConsoleWidth, info.ContinuationPromptWidth);
        return (offset.X, info.StartTop + offset.Y);
    }
}
