using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Metatool.Service.MouseKey;

/// <summary>
/// https://www.w3.org/TR/uievents-key/
/// </summary>
// [DebuggerDisplay("{this}")]
public partial class Key : IKey, IComparable, IComparable<Key>, ISequenceUnit, ISequencable
{
    private SortedSet<KeyValues> _codes;
    private int _val;

    public SortedSet<KeyValues> Codes
    {
        get => _codes;
        private set
        {
            _codes = value;
            _val = value.Aggregate<KeyValues, int>(0, (o, c1) => o + (int)c1);
        }
    }

    public Key(params KeyValues[] keyCodes)
    {
        Codes = new(keyCodes);
    }

    public Key(params Key[] keys)
    {
        Codes = new(keys.SelectMany(k => k.Codes));
        Handled = keys.Aggregate(Handled, (a, c) => a |= c.Handled);
    }

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