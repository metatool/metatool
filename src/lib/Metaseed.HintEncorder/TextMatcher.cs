namespace Metaseed.HintEncorder;

/// <summary>
/// Finds character/string matches within buffer text.
/// Equivalent to Get-Matches, Get-ContinueRippleTargets, Test-PartialMatch.
/// </summary>
public static class TextMatcher
{
    /// <summary>
    /// Returns all start indexes (0-based) where <paramref name="targetFilterText"/>
    /// appears in <paramref name="line"/> (case-insensitive).
    /// </summary>
    public static int[] GetMatches(string line, string targetFilterText)
    {
        if (string.IsNullOrEmpty(targetFilterText))
            return [];

        var indexes = new List<int>();
        var index = 0;
        while (true)
        {
            index = line.IndexOf(targetFilterText, index, StringComparison.OrdinalIgnoreCase);
            if (index == -1) break;
            indexes.Add(index);
            index++;
        }
        return indexes.ToArray();
    }

    /// <summary>
    /// Filters <paramref name="targetMatchIndexes"/> to only those where the character
    /// at <paramref name="inputCharOffset"/> positions after the match start equals <paramref name="inputChar"/>.
    /// Used to narrow matches as the user types additional characters.
    /// </summary>
    public static int[] GetContinueRippleTargets(
        string inputChar, string bufferText, int[] targetMatchIndexes, int inputCharOffset)
    {
        var result = new List<int>();
        foreach (var idx in targetMatchIndexes)
        {
            var nextIdx = idx + inputCharOffset;
            if (nextIdx < bufferText.Length && inputChar == bufferText[nextIdx].ToString())
                result.Add(idx);
        }
        return result.ToArray();
    }

    /// <summary>
    /// Returns true if any code in <paramref name="codes"/> starts with <paramref name="inputCode"/>.
    /// </summary>
    public static bool TestPartialMatch(string[] codes, string inputCode)
    {
        foreach (var c in codes)
        {
            if (c.StartsWith(inputCode, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}
