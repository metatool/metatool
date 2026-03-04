namespace Metaseed.HintEncorder;

/// <summary>
/// Core jump-code generation algorithm. Translates the encoder.ps1 logic.
/// Generates short, non-conflicting hint codes for a set of match positions.
/// </summary>
public static class HintEncoder
{
    /// <summary>
    /// Filters <paramref name="codeChars"/> to exclude any character that appears
    /// immediately after one of the matched filter regions, so that typing a code
    /// character cannot be confused with continuing the ripple filter.
    /// </summary>
    public static string[] GetUsableFirstDimensionCodeChars(
        string[] codeChars, string bufferText, int targetTextLength, int[] targetMatchIndexes)
    {
        var usable = new List<string>();
        foreach (var charCode in codeChars)
        {
            var keep = true;
            foreach (var idx in targetMatchIndexes)
            {
                var nextIdx = idx + targetTextLength;
                if (nextIdx < bufferText.Length && charCode == bufferText[nextIdx].ToString())
                {
                    keep = false;
                    break;
                }
            }
            if (keep)
                usable.Add(charCode);
        }
        return usable.ToArray();
    }

    /// <summary>
    /// Generates jump codes using multi-dimensional encoding.
    /// The first dimension uses <paramref name="firstDimCodeChars"/>,
    /// subsequent dimensions use <paramref name="commonDimCodeChars"/>.
    /// Short (1-char) codes are preferred; longer codes are generated as needed.
    /// </summary>
    public static string[] GetJumpCodes(
        int[] targetMatchIndexes,
        string[] firstDimCodeChars,
        string[] commonDimCodeChars,
        string[] additionalSingleCodeChars)
    {
        var targetCount = targetMatchIndexes.Length;
        if (targetCount == 0)
            return [];

        var commonDimLen = commonDimCodeChars.Length;
        var firstCharCodeSetCount = firstDimCodeChars.Length;

        // 1-len codes: firstDimCodeChars + additionalSingleCodeChars
        if (targetCount <= firstCharCodeSetCount + additionalSingleCodeChars.Length)
        {
            var singleCharCodes = firstDimCodeChars.Concat(additionalSingleCodeChars).ToArray();
            return singleCharCodes[..targetCount];
        }

        var remainTargetCount = targetCount - additionalSingleCodeChars.Length;
        var remainDimensionsShares = (double)remainTargetCount / firstCharCodeSetCount;
        var dimensions = (int)Math.Ceiling(Math.Log(remainDimensionsShares, commonDimLen)) + 1;
        var midDims = dimensions - 2; // first dimension - 1, highest dimension - 1
        var lowDimsElementCount = (int)Math.Ceiling(firstCharCodeSetCount * Math.Pow(commonDimLen, midDims));
        var usedInLowDimCount = (int)Math.Ceiling(
            (double)(remainTargetCount - lowDimsElementCount) / (commonDimLen - 1));

        List<string> GetCodeOfFullDimensions(int skips = 0, int dimensionCount = -1, int totalCodesToGet = -1)
        {
            if (dimensionCount < 0)
                dimensionCount = dimensions;
            if (totalCodesToGet == 0)
                return [];
            if (totalCodesToGet < 0)
                totalCodesToGet = (int)(firstCharCodeSetCount * Math.Pow(commonDimLen, dimensionCount - 1));

            var fullDimCodes = new List<string>();
            var dimensionList = new int[dimensionCount];
            dimensionList[0] = firstCharCodeSetCount;
            for (var i = 1; i < dimensionCount; i++)
                dimensionList[i] = commonDimLen;

            var skipsCounter = 0;

            CombinationIterator.ForEachCombination(dimensionList, indices =>
            {
                if (skipsCounter < skips)
                {
                    skipsCounter++;
                    return true;
                }

                var code = firstDimCodeChars[indices[0]];
                for (var j = 1; j < indices.Length; j++)
                    code += commonDimCodeChars[indices[j]];

                fullDimCodes.Add(code);
                return fullDimCodes.Count < totalCodesToGet;
            });

            return fullDimCodes;
        }

        var codes = new List<string>();

        if (dimensions == 2)
        {
            // remaining single-char codes (lower dimension) + additional single-char codes + high dimension codes
            if (usedInLowDimCount <= lowDimsElementCount)
            {
                for (var i = usedInLowDimCount; i < lowDimsElementCount && i < firstDimCodeChars.Length; i++)
                    codes.Add(firstDimCodeChars[i]);
                if (codes.Count >= targetCount)
                    return codes[..targetCount].ToArray();
            }
            codes.AddRange(additionalSingleCodeChars);
            if (codes.Count >= targetCount)
                return codes[..targetCount].ToArray();

            codes.AddRange(GetCodeOfFullDimensions(totalCodesToGet: targetCount - codes.Count));
            return codes.ToArray();
        }

        // dimensions >= 3: additional single-char codes + lower dimensional codes + high dimension codes
        codes.AddRange(additionalSingleCodeChars);
        if (codes.Count >= targetCount)
            return codes[..targetCount].ToArray();

        codes.AddRange(GetCodeOfFullDimensions(
            skips: usedInLowDimCount,
            dimensionCount: dimensions - 1,
            totalCodesToGet: targetCount - codes.Count));

        codes.AddRange(GetCodeOfFullDimensions(totalCodesToGet: targetCount - codes.Count));

        return codes.ToArray();
    }

    /// <summary>
    /// Generates jump codes for a ripple wave, filtering out code chars that conflict
    /// with the next character after filter matches. Throws if no usable code chars remain.
    /// </summary>
    public static string[] GetJumpCodesForWave(
        string[] codeChars,
        int[] targetMatchIndexes,
        string bufferText,
        int targetTextLength,
        string[] additionalSingleCodeChars,
        int cursorIndex = 0)
    {
        var usableFirst = GetUsableFirstDimensionCodeChars(
            codeChars, bufferText, targetTextLength, targetMatchIndexes);

        if (usableFirst.Length == 0)
            throw new InvalidOperationException(
                "Please continue ripple-typing, or press 'Enter' then navigating. " +
                "(All code chars used by following chars, no enough code chars)");

        var codes = GetJumpCodes(targetMatchIndexes, usableFirst, codeChars, additionalSingleCodeChars);

        if (codes.Length != targetMatchIndexes.Length)
            throw new InvalidOperationException(
                $"MetaJump: Code count mismatch: codes({codes.Length}) != TargetMatchIndexes({targetMatchIndexes.Length})");

        return AlignCodesAroundCursor(codes, targetMatchIndexes, cursorIndex);
    }

    /// <summary>
    /// Reorders codes so that the shortest (easiest-to-type) codes are assigned
    /// to match positions nearest the cursor, alternating left and right.
    /// </summary>
    public static string[] AlignCodesAroundCursor(string[] codes, int[] targetMatchIndexes, int cursorIndex)
    {
        if (cursorIndex == 0)
            return codes;

        if (cursorIndex >= targetMatchIndexes[^1])
        {
            var reversed = new string[codes.Length];
            Array.Copy(codes, reversed, codes.Length);
            Array.Reverse(reversed);
            return reversed;
        }

        var newCodes = new string[codes.Length];

        // Find the index of the first match that is after the cursor
        var left = 0;
        for (var i = 0; i < targetMatchIndexes.Length; i++)
        {
            if (targetMatchIndexes[i] > cursorIndex)
            {
                left = i;
                break;
            }
        }

        var right = left + 1;
        var index = 0;

        while (left >= 0 && right < targetMatchIndexes.Length)
        {
            newCodes[left] = codes[index];
            newCodes[right] = codes[index + 1];
            left--;
            right++;
            index += 2;
        }

        while (left >= 0)
        {
            newCodes[left] = codes[index];
            left--;
            index++;
        }

        while (right < targetMatchIndexes.Length)
        {
            newCodes[right] = codes[index];
            right++;
            index++;
        }

        return newCodes;
    }
}
