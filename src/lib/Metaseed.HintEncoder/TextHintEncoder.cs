namespace Metaseed.Hint;

public static class TextHintEncoder
{
    /// <summary>
    /// Generates jump codes for a wave, filtering out code chars that conflict with
    /// the character following each match, then aligning codes around the cursor.
    /// </summary>
    public static List<string> GetJumpCodesForWave(
        string[] codeChars, int[] targetMatchIndexes, string text, int textLength,
        string[]? additionalSingleCodeChars = null, int cursorIndex = 0)
    {
        additionalSingleCodeChars ??= [];

        var usableCodeCharsOfFirstDim = GetUsableFirstDimensionCodeChars(
            codeChars, text, targetMatchIndexes, textLength);

        if (usableCodeCharsOfFirstDim.Count == 0)
            throw new InvalidOperationException(
                "Please continue ripple-typing, or press 'enter' then navigating. " +
                "(all code chars used by following chars, no enough code chars)");

        var codes = HintEncoder.GetJumpCodes(targetMatchIndexes.Length, usableCodeCharsOfFirstDim.ToArray(), codeChars, additionalSingleCodeChars);

        return HintEncoder.AlignCodesAroundFocus(codes, targetMatchIndexes, cursorIndex);
    }

    /// <summary>
    /// Filters code chars to exclude those that case-insensitively match the character
    /// immediately following any target match in the buffer text.
    /// </summary>
    private static List<string> GetUsableFirstDimensionCodeChars(
        string[] codeChars, string text, int[] targetMatchIndexes, int textLength)
    {
        var usableCodeChars = new List<string>();
        foreach (var charCode in codeChars)
        {
            var keep = true;
            foreach (var idx in targetMatchIndexes)
            {
                var nextIndex = idx + textLength;
                if (nextIndex < text.Length)
                {
                    var nextChar = text[nextIndex];
                    // case-insensitive compare, so user can type either case
                    // if T is next char, and t in code, we should filter out of the code t
                    if (char.ToUpperInvariant(charCode[0]) == char.ToUpperInvariant(nextChar))
                    {
                        keep = false;
                        break;
                    }
                }
            }
            if (keep)
            {
                usableCodeChars.Add(charCode);
            }
        }
        return usableCodeChars;
    }
}