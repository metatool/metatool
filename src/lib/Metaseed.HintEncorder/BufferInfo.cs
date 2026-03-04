namespace Metaseed.HintEncorder;

public class BufferInfo
{
    /// <summary>Buffer text including '\n'.</summary>
    public string Line { get; init; } = "";

    /// <summary>Cursor position in the whole text, 0-based.</summary>
    public int Cursor { get; init; }

    /// <summary>Cursor X position in console, 0-based.</summary>
    public int ConsoleLeft { get; init; }

    /// <summary>Cursor Y position in console, 0-based.</summary>
    public int ConsoleTop { get; init; }

    /// <summary>Console window width in characters.</summary>
    public int ConsoleWidth { get; init; }

    /// <summary>The buffer's first line's X position, 0-based.</summary>
    public int StartLeft { get; init; }

    /// <summary>The buffer's first line's Y position, 0-based.</summary>
    public int StartTop { get; init; }

    /// <summary>Width of continuation prompt (e.g. ">> ").</summary>
    public int ContinuationPromptWidth { get; init; }

    public static (int X, int Y) GetVisualOffset(string line, int index, int startLeft, int bufferWidth, int continuationPromptWidth = 0)
    {
        var x = startLeft;
        var y = 0;

        for (var i = 0; i < index; i++)
        {
            var c = line[i];
            if (c == '\n')
            {
                x = continuationPromptWidth;
                y++;
            }
            else if (c == '\r')
            {
                x = 0;
            }
            else
            {
                x++;
                if (x >= bufferWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }

        return (x, y);
    }

    public static BufferInfo Create(IBufferProvider provider)
    {
        var (line, cursor) = provider.GetBufferState();
        var consoleLeft = Console.CursorLeft;
        var consoleTop = Console.CursorTop;
        var bufferWidth = Console.BufferWidth;
        var continuationPromptWidth = provider.ContinuationPromptWidth;

        int startLeft, startTop;
        var substring = line[..cursor];

        if (substring.Contains('\n'))
        {
            provider.SetCursorPosition(0);
            startLeft = Console.CursorLeft;
            provider.SetCursorPosition(cursor);

            var offset = GetVisualOffset(line, cursor, startLeft, bufferWidth, continuationPromptWidth);
            startTop = consoleTop - offset.Y;
        }
        else
        {
            startLeft = consoleLeft - cursor;
            startTop = consoleTop;

            while (startLeft < 0)
            {
                startLeft += bufferWidth;
                startTop--;
            }
        }

        return new BufferInfo
        {
            Line = line,
            Cursor = cursor,
            ConsoleLeft = consoleLeft,
            ConsoleTop = consoleTop,
            ConsoleWidth = bufferWidth,
            StartLeft = startLeft,
            StartTop = startTop,
            ContinuationPromptWidth = continuationPromptWidth
        };
    }
}
