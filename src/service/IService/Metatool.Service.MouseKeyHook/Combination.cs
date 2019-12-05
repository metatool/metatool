using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Metatool.Service.MouseKeyHook.Implementation;
using Microsoft.Extensions.Logging;

namespace Metatool.Service
{
    /// <summary>
    ///     Used to represent a key combination as frequently used in application as shortcuts.
    ///     e.g. Alt+Shift+R. This combination is triggered when 'R' is pressed after 'Alt' and 'Shift' are already down.
    /// </summary>
    public partial class Combination : ICombination
    {
        private readonly Chord _chord;
        private          Key   _key;

        public Combination(Keys triggerKey) : this(triggerKey, null as Chord)
        {
        }

        public Combination(Keys triggerKey, Keys chordKey) : this(triggerKey, new Keys[] {chordKey})
        {
        }


        public Combination(Keys triggerKey, IEnumerable<Keys> chordKeys)
            : this(triggerKey, new Chord(chordKeys ?? Enumerable.Empty<Keys>()))
        {
        }

        public Combination(Key triggerKey) : this(triggerKey, null as Chord)
        {
        }

        public Combination(Key triggerKey, Key chordKey) : this(triggerKey, new Key[] {chordKey})
        {
        }

        public Combination(Key triggerKey, IEnumerable<Key> chordKeys) : this(triggerKey,
            new Chord(chordKeys ?? Enumerable.Empty<Key>()))
        {
        }

        private Combination(Keys triggerKey, Chord chord) : this((Key) triggerKey, chord)
        {
        }

        public Combination(Key triggerKey, ISequenceUnit sequenceUnit): this(triggerKey, sequenceUnit.ToCombination().AllKeys)
        {
        }


        private Combination(Key triggerKey, Chord chord)
        {
            TriggerKey = triggerKey;
            _chord     = chord ?? new Chord(Enumerable.Empty<Keys>());
            _key       = TriggerKey;
        }


        public Key TriggerKey { get; }

        public IEnumerable<Key> Chord => _chord;

        public int ChordLength => _chord.Count;

        public IEnumerable<Key> AllKeys => _chord.Append(TriggerKey);

        public bool IsAnyKey(Keys key)
        {
            if (key == TriggerKey) return true;
            if (Chord.Contains(key)) return true;
            return false;
        }


        public override string ToString()
        {
            return string.Join(" + ", Chord.Concat(Enumerable.Repeat(TriggerKey, 1)));
        }

        public static Combination Parse(string str)
        {
            var parts = str
                .Split('+', StringSplitOptions.RemoveEmptyEntries)
                .Select(p =>Key.Parse(p.Trim()));
            var stack      = new Stack<Key>(parts);
            var triggerKey = stack.Pop();
            return new Combination(triggerKey, stack);
        }

        public static bool TryParse(string str, out Combination value, bool log=true)
        {
            try
            {
                value = Parse(str);
                return true;
            }
            catch (Exception e)
            {
                if(log) Services.CommonLogger?.LogError(e.Message);
                value = null;
                return false;
            }
        }
    }
}
