namespace Metaseed.HintEncorder;

/// <summary>
/// Iterates through all combinations of indices for given dimension sizes,
/// like an odometer. Equivalent to the PowerShell ForEach-Combination.
/// </summary>
public static class CombinationIterator
{
    /// <summary>
    /// Iterates all index combinations for the given <paramref name="dimensions"/>.
    /// <paramref name="action"/> receives the current index array; return false to break.
    /// </summary>
    public static void ForEachCombination(int[] dimensions, Func<int[], bool> action)
    {
        var indices = new int[dimensions.Length];

        while (true)
        {
            if (!action(indices))
                break;

            // Increment indices (odometer-style, rightmost = highest dimension)
            var pos = dimensions.Length - 1;
            while (pos >= 0)
            {
                indices[pos]++;
                if (indices[pos] < dimensions[pos])
                    break;
                indices[pos] = 0;
                pos--;
            }

            if (pos < 0)
                break;
        }
    }
}
