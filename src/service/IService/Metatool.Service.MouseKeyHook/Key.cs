using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Metatool.Service.MouseKey;

/// <summary>
/// https://www.w3.org/TR/uievents-key/
/// A|B, A|B*, A*|B*
/// </summary>
// [DebuggerDisplay("{this}")]
public partial class Key : IKey, IComparable, IComparable<Key>, ISequenceUnit, ISequencable
{
    private SortedSet<KeyCodes> _codes;
    private int _val;
    /// <summary>
    /// codes of A|B|...
    /// </summary>
    public SortedSet<KeyCodes> Codes
    {
        get => _codes;
        private set
        {
            _codes = value;
            _val = value.Aggregate<KeyCodes, int>(0, (o, c1) => o + (int)c1);
        }
    }

    public Key(params KeyCodes[] keyCodes)
    {
        Codes = new(keyCodes);
    }

    public Key(params Key[] keys)
    {
        Codes = new(keys.SelectMany(k => k.Codes));
        Handled = keys.Aggregate(Handled, (a, c) => a |= c.Handled);
    }
    /// <summary>
    /// A|B, A|B*, A*|B*
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Key Parse(string str)
    {
        var handled = KeyEventType.None;
        var keys = str.Split("|", StringSplitOptions.RemoveEmptyEntries).Select(s =>
        {
            s = s.Trim();
            if (s.EndsWith('*')) // todo: *{Up&Down} only mark Up and Down event handled
            {
                handled = KeyEventType.All;
                s = s.TrimEnd('*').TrimEnd();
            }
            var r = All.TryGetValue(s, out var key);
            if (r) return key;

            throw new($"Could not parse Key '{s}', in string {str}.");
        }).ToArray();

        return new(keys) { Handled = handled };
    }

    public static bool TryParse(string str, out Key value, bool log = true)
    {
        try
        {
            value = Parse(str);
            return true;
        }
        catch (Exception e)
        {
            if (log) Services.CommonLogger?.LogError(
                e.Message + $"{Environment.NewLine} Please use the following Keys: {string.Join(",", AllNames)}");
            value = null;
            return false;
        }
    }

    public override string ToString()
    {
        return $"{string.Join("|", Codes)}{(Handled != KeyEventType.None ? $"*{{{Handled.ToString()}}}" : "")}";
    }
}