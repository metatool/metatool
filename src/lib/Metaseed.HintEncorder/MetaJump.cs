namespace Metaseed.HintEncorder;

/// <summary>
/// Main orchestrator: implements the Ripple → Navigate flow for jumping
/// to any character in a console input buffer. Equivalent to Invoke-MetaJump.ps1.
/// </summary>
public class MetaJump
{
    private readonly IBufferProvider _bufferProvider;
    private readonly MetaJumpConfig _config;

    public MetaJump(IBufferProvider bufferProvider, MetaJumpConfig? config = null)
    {
        _bufferProvider = bufferProvider;
        _config = config ?? new MetaJumpConfig();
    }

    /// <summary>Entry point. Reads the buffer, runs Ripple then Navigate, restores visuals.</summary>
    public void Invoke()
    {
        var info = BufferInfo.Create(_bufferProvider);
        if (string.IsNullOrEmpty(info.Line))
            return;

        var cursorVisible = Console.CursorVisible;
        Console.CursorVisible = false;

        try
        {
            var rippleResult = Ripple(info);
            if (rippleResult == null)
                return; // cancelled

            Navigate(
                rippleResult.TargetMatchIndexes,
                rippleResult.Codes,
                rippleResult.FilterLength,
                info,
                rippleResult.InitialKey);
        }
        finally
        {
            Console.CursorVisible = cursorVisible;
            ConsoleRenderer.RestoreVisuals(info, _bufferProvider);
        }
    }

    /// <summary>
    /// Reads a key while optionally showing a tooltip and start indicator.
    /// Equivalent to Get-TargetChar.
    /// </summary>
    private ConsoleKeyInfo GetTargetChar(BufferInfo info, string icon = "", string tooltip = "")
    {
        var tooltipTop = -1;
        var indicatorLen = 0;

        try
        {
            if (!string.IsNullOrEmpty(tooltip))
            {
                tooltipTop = ConsoleTooltip.GetTooltipTop(info);
                ConsoleTooltip.Show(tooltipTop, tooltip);
            }
            if (!string.IsNullOrEmpty(icon))
            {
                indicatorLen = ConsoleIndicator.ShowStartIndicator(info, icon);
            }

            return Console.ReadKey(true);
        }
        finally
        {
            if (tooltipTop >= 0)
                ConsoleTooltip.Clear(tooltipTop);
            if (indicatorLen > 0)
                ConsoleIndicator.RestoreStartIndicator(info, indicatorLen);
        }
    }

    /// <summary>
    /// Phase 1: Interactive filtering. User types characters to narrow matches.
    /// Returns null if cancelled (Escape). Transitions to Navigate when a code
    /// prefix is matched or Enter is pressed.
    /// </summary>
    private RippleResult? Ripple(BufferInfo info)
    {
        var filterText = "";
        var codes = Array.Empty<string>();
        var targetMatchIndexes = Array.Empty<int>();
        var errorMsg = "";

        while (true)
        {
            string icon, tooltip;
            if (!string.IsNullOrEmpty(errorMsg))
            {
                icon = "\u26A0\uFE0F"; // ⚠️
                tooltip = errorMsg;
                errorMsg = "";
            }
            else if (filterText.Length == 0)
            {
                icon = "\U0001F3C3"; // 🏃
                tooltip = "MetaJump: Please type target char to jump...";
            }
            else
            {
                icon = "";
                tooltip = "MetaJump: Please type code to jump to or continue typing chars following target char...";
            }

            var key = GetTargetChar(info, icon, tooltip);

            if (key.Key == ConsoleKey.Escape)
                return null;

            if (key.Key == ConsoleKey.Enter || TextMatcher.TestPartialMatch(codes, key.KeyChar.ToString()))
            {
                return new RippleResult(targetMatchIndexes, codes, filterText.Length, key);
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                // Backspace currently not implemented (matches PS commented-out code)
                continue;
            }

            var ch = key.KeyChar.ToString();

            // Find or refine matches
            if (targetMatchIndexes.Length == 0)
            {
                targetMatchIndexes = TextMatcher.GetMatches(info.Line, ch);
            }
            else
            {
                targetMatchIndexes = TextMatcher.GetContinueRippleTargets(
                    ch, info.Line, targetMatchIndexes, filterText.Length);
            }

            if (targetMatchIndexes.Length > 0)
            {
                filterText += ch;
            }
            else
            {
                Console.Beep();
                errorMsg = "MetaJump: No matches for last character";
                continue;
            }

            // Generate codes
            try
            {
                codes = HintEncoder.GetJumpCodesForWave(
                    _config.CodeChars,
                    targetMatchIndexes,
                    info.Line,
                    filterText.Length,
                    _config.AdditionalSingleCodeChars,
                    info.Cursor);
            }
            catch (InvalidOperationException ex)
            {
                errorMsg = ex.Message;
                continue;
            }

            // Draw overlay
            ConsoleRenderer.DrawOverlay(info, targetMatchIndexes, codes, filterText.Length, _config);
        }
    }

    /// <summary>
    /// Phase 2: User types code characters to select a target.
    /// Shrinks the code set with each keystroke. When only one match remains,
    /// jumps to the target position.
    /// </summary>
    private void Navigate(int[] targetMatchIndexes, string[] codes, int filterLength,
        BufferInfo info, ConsoleKeyInfo? initialKey)
    {
        var guidingInfo = "MetaJump: Type codes to jump to target, or 'Esc' to cancel.";
        var icon = "\u2139\uFE0F"; // ℹ️
        var tooltip = guidingInfo;
        var firstLoop = true;

        while (true)
        {
            // If only one match and code remain, jump to target
            if (codes.Length == 1 && targetMatchIndexes.Length == 1)
            {
                _bufferProvider.SetCursorPosition(targetMatchIndexes[0] + 1); // behind the char
                return;
            }

            ConsoleKeyInfo key;
            if (firstLoop && initialKey.HasValue && initialKey.Value.Key != ConsoleKey.Enter)
            {
                key = initialKey.Value;
                firstLoop = false;
            }
            else
            {
                key = GetTargetChar(info, icon, tooltip);
                if (key.Key == ConsoleKey.Escape)
                    return;

                var potentialCode = key.KeyChar.ToString();
                if (!TextMatcher.TestPartialMatch(codes, potentialCode))
                {
                    Console.Beep();
                    icon = "\u26A0\uFE0F"; // ⚠️
                    tooltip = "MetaJump: No matches for last character, please type code on screen or 'Esc' to cancel.";
                    continue;
                }
            }

            // Shrink codes
            var keyChar = key.KeyChar.ToString();
            var newCodes = new List<string>();
            var newTargetMatchIndexes = new List<int>();

            for (var i = 0; i < codes.Length; i++)
            {
                var c = codes[i];
                if (c.Length == 0)
                {
                    if (string.Equals(c, keyChar, StringComparison.Ordinal))
                    {
                        newCodes = [c];
                        newTargetMatchIndexes = [targetMatchIndexes[i]];
                        break;
                    }
                }
                else
                {
                    if (c.StartsWith(keyChar, StringComparison.Ordinal))
                    {
                        newTargetMatchIndexes.Add(targetMatchIndexes[i]);
                        newCodes.Add(c[1..]);
                    }
                }
            }

            targetMatchIndexes = newTargetMatchIndexes.ToArray();
            codes = newCodes.ToArray();

            ConsoleRenderer.DrawOverlay(info, targetMatchIndexes, codes, filterLength, _config, isRipple: false);

            icon = "\u2139\uFE0F"; // ℹ️
            tooltip = guidingInfo;
        }
    }

    private record RippleResult(int[] TargetMatchIndexes, string[] Codes, int FilterLength, ConsoleKeyInfo InitialKey);
}
