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

            if (TryAdjustLowerCase(s, out var k)) return k;

            throw new($"Could not parse Key '{s}', in string {str}.");
        }).ToArray();

        return new(keys) { Handled = handled };
    }

    /// parse 'a' to 'z' and 'shift','alt','ctrl' witch only first letter is not uppercase
    private static bool TryAdjustLowerCase(string s, out Key key)
    {
        if (s.Length == 1)
        {
            if (s[0] >= 'a' && s[0] <= 'z')
            {
                s = s.ToUpper();
            }
        }
        else
        {
            // shift
            s = UppercaseFirst(s);
        }

        var r = All.TryGetValue(s, out key);
        return r;
    }

    private static string UppercaseFirst(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return char.ToUpper(text[0]) + text.Substring(1);
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

    string _name;
    public string KeyName
    {
        get
        {
            if(_name != null) return _name;

            var codesClone = new SortedSet<KeyCodes>(Codes);
            if (codesClone.Contains(KeyCodes.LShiftKey) && codesClone.Contains(KeyCodes.RShiftKey))
            {
                codesClone = new(codesClone.Except([KeyCodes.LShiftKey, KeyCodes.RShiftKey]));
                codesClone.Add(KeyCodes.ShiftKey);
            }
            if (codesClone.Contains(KeyCodes.LControlKey) && codesClone.Contains(KeyCodes.RControlKey))
            {
                codesClone = new(codesClone.Except([KeyCodes.LControlKey, KeyCodes.RControlKey]));
                codesClone.Add(KeyCodes.ControlKey);
            }
            if (codesClone.Contains(KeyCodes.LMenu) && codesClone.Contains(KeyCodes.RMenu))
            {
                codesClone = new(codesClone.Except([KeyCodes.LMenu, KeyCodes.RMenu]));
                codesClone.Add(KeyCodes.Menu);
            }

            _name = $"{string.Join("|", codesClone.Select(c => c.KeyName()))}";
            return _name;
        }
    }


    public override string ToString()
    {
        return $"{KeyName}{(Handled != KeyEventType.None ? $"*{{{Handled.ToString()}}}" : "")}";
    }
}