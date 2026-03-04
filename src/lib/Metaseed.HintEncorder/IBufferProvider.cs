namespace Metaseed.HintEncorder;

/// <summary>
/// Abstracts the buffer/cursor provider (e.g. PSConsoleReadLine) so the library
/// can be used from any console host.
/// </summary>
public interface IBufferProvider
{
    /// <summary>Returns the current buffer text and cursor position (0-based index into the text).</summary>
    (string line, int cursor) GetBufferState();

    /// <summary>Moves the cursor to <paramref name="position"/> (0-based index into the buffer text).</summary>
    void SetCursorPosition(int position);

    /// <summary>Inserts text at the current cursor position (used to trigger a host refresh).</summary>
    void Insert(string text);

    /// <summary>Gets the continuation prompt width (e.g. ">> ".Length) for multi-line input.</summary>
    int ContinuationPromptWidth { get; }
}
