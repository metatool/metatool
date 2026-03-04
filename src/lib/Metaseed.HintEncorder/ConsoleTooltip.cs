namespace Metaseed.HintEncorder;

/// <summary>
/// Shows and clears tooltip text below the input buffer.
/// Equivalent to Tooltip.ps1.
/// </summary>
public static class ConsoleTooltip
{
    public static void Show(int top, string text)
    {
        if (top >= Console.BufferHeight)
            return;

        Console.SetCursorPosition(0, top);
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    public static void Clear(int top)
    {
        if (top < Console.BufferHeight)
        {
            Console.SetCursorPosition(0, top);
            Console.Write(AnsiColor.ClearToEndOfLine);
        }
    }

    public static int GetTooltipTop(BufferInfo info)
    {
        var endOffset = BufferInfo.GetVisualOffset(
            info.Line, info.Line.Length, info.StartLeft, info.ConsoleWidth, info.ContinuationPromptWidth);
        return info.StartTop + endOffset.Y + 1;
    }
}
