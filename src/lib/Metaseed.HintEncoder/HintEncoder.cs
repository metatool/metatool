using System;
using System.Collections.Generic;

namespace Metaseed.Hint;

public static class HintEncoder
{
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

    public static List<string> GetJumpCodes(
    int targetCount, string[] codeChars, string[]? additionalSingleCodeChars = null)
        => GetJumpCodes(targetCount, codeChars, codeChars, additionalSingleCodeChars);

    /// <summary>
    /// Generates jump codes. The first dimension uses firstDimCodeChars, remaining dimensions use commonDimCodeChars.
    /// Shape: [firstDimLen, commonDimLen, commonDimLen, ..., commonDimLen]
    /// </summary>
    public static List<string> GetJumpCodes(
        int targetCount, string[] firstDimCodeChars, string[] commonDimCodeChars,
        string[]? additionalSingleCodeChars = null)
    {
        additionalSingleCodeChars ??= [];

        if (targetCount == 0) return [];

        var firstDimLen = firstDimCodeChars.Length;
        var commonDimLen = commonDimCodeChars.Length;
        var remainTargetCount = targetCount - additionalSingleCodeChars.Length;

        // 1-len codes: firstDimCodeChars + additionalSingleCodeChars
        // dimensions = 1
        if (targetCount <= firstDimLen + additionalSingleCodeChars.Length)
        {
            // single char codes
            var singleCodes = new List<string>(firstDimCodeChars);
            singleCodes.AddRange(additionalSingleCodeChars);
            return singleCodes.GetRange(0, targetCount);
        }

        // remainTargetCount > 0 and > firstDimLen
        // dimensions >= 2
        var countPerFirstDimElem = (double)remainTargetCount / firstDimLen;
        var dimensions = (int)Math.Ceiling(Math.Log(countPerFirstDimElem, commonDimLen)) + 1;
        var midDims = dimensions - 1 /*first dimension*/ - 1 /*highest dimension*/;
        var lowDimsElementCount = firstDimLen * (int)Math.Pow(commonDimLen, midDims);
        var usedInLowDimCount = (int)Math.Ceiling(
            (double)(remainTargetCount - lowDimsElementCount) / (commonDimLen - 1));

        List<string> GetCodeOfFullDimensions(int skipsFromTotal, int dimensionCount, int totalCodesToGet = -1)
        {
            if (totalCodesToGet == -1)
                totalCodesToGet = firstDimLen * (int)Math.Pow(commonDimLen, dimensionCount - 1);
            if (totalCodesToGet <= 0)
                return [];

            var fullDimCodes = new List<string>();
            var dimensionList = new int[dimensionCount];
            dimensionList[0] = firstDimLen;
            for (var i = 1; i < dimensionCount; i++)
                dimensionList[i] = commonDimLen;

            ForEachCombination(dimensionList, indexes =>
            {
                var code = firstDimCodeChars[indexes[0]];
                for (var j = 1; j < indexes.Length; j++)
                    code += commonDimCodeChars[indexes[j]];

                fullDimCodes.Add(code);
                return fullDimCodes.Count != totalCodesToGet; // false breaks
            }, skipsFromTotal);

            return fullDimCodes;
        }

        var codesRt = new List<string>();

        if (dimensions == 2)
        {
            // remaining single char codes (lower dimension) + additional + high dimension codes
            if (usedInLowDimCount < lowDimsElementCount)
            {
                for (var i = usedInLowDimCount; i < lowDimsElementCount; i++)
                    codesRt.Add(firstDimCodeChars[i]);
            }
            codesRt.AddRange(additionalSingleCodeChars);
            codesRt.AddRange(GetCodeOfFullDimensions(0, dimensions, totalCodesToGet: targetCount - codesRt.Count));
            return codesRt;
        }

        // dimensions >= 3: additional single char codes + lower dimensional codes + high dimension codes
        codesRt.AddRange(additionalSingleCodeChars);
        codesRt.AddRange(GetCodeOfFullDimensions(skipsFromTotal: usedInLowDimCount, dimensions - 1));
        codesRt.AddRange(GetCodeOfFullDimensions(skipsFromTotal: 0, dimensions, totalCodesToGet: targetCount - codesRt.Count));

        return codesRt;
    }

    /// <summary>
    /// Reorders codes so that shorter (easier) codes are assigned to matches
    /// closer to the cursor position.
    /// </summary>
    public static List<string> AlignCodesAroundFocus(List<string> codes, int[] targetMatchIndexes, int focusIndex)
    {
        if (codes.Count != targetMatchIndexes.Length)
            throw new ArgumentException(
                $"Code count mismatch: codes({codes.Count}) != targetMatchIndexes({targetMatchIndexes.Length})");

        if (focusIndex <= targetMatchIndexes[0])
            return codes;
        if (focusIndex >= targetMatchIndexes[^1])
        {
            codes.Reverse();
            return codes;
        }

        // cursorIndex is in the middle, targetMatchIndexes.Length >= 2
        var newCodes = new List<string>(codes.Count);

        // find the index of the last match before or at the cursor
        var left = 0;
        for (var i = 0; i < targetMatchIndexes.Length; i++)
        {
            if (targetMatchIndexes[i] > focusIndex)
            {
                left = i - 1;
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

    /// <summary>
    /// Iterates over all combinations of indices for the given dimensions (odometer-style).
    /// Return false from the action to break early.
    /// </summary>
    private static void ForEachCombination(int[] dimensions, Func<int[], bool> action, int skips = 0)
    {
        var indices = new int[dimensions.Length];

        while (true)
        {
            if (skips > 0) { skips--; continue; }
            var continueLoop = action(indices);
            if (!continueLoop) break;

            // Increment indices (odometer from the highest index)
            var pos = dimensions.Length - 1;
            while (pos >= 0)
            {
                indices[pos]++;
                if (indices[pos] < dimensions[pos]) break;
                indices[pos] = 0;
                pos--;
            }

            if (pos < 0) break;
        }
    }
}
