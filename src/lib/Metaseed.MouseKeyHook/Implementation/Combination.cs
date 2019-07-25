// This code is distributed under MIT license. 
// Copyright (c) 2010-2018 George Mamaladze
// See license.txt or https://mit-license.org/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input.MouseKeyHook
{
    /// <summary>
    ///     Used to represent a key combination as frequently used in application as shortcuts.
    ///     e.g. Alt+Shift+R. This combination is triggered when 'R' is pressed after 'Alt' and 'Shift' are already down.
    /// </summary>
    public partial class Combination : ICombination
    {
        private readonly Chord _chord;
        private          Keys  _key;

        public Combination(Keys triggerKey) : this(triggerKey, null)
        {
        }

        public Combination(Keys triggerKey, Keys chordKey) : this(triggerKey, new Keys[] {chordKey})
        {
        }

        public Combination(Keys triggerKey, IEnumerable<Keys> chordKeys)
            : this(triggerKey, new Chord(chordKeys ?? Enumerable.Empty<Keys>()))
        {
        }

        private Combination(Keys triggerKey, Chord chord)
        {
            TriggerKey = triggerKey;
            _chord     = chord ?? new Chord(Enumerable.Empty<Keys>());
            _key       = TriggerKey;
        }


        /// <summary>
        ///     Last key which triggers the combination.
        /// </summary>
        public Keys TriggerKey { get; }

        /// <summary>
        ///     Keys which all must be alredy down when trigger key is pressed.
        /// </summary>
        public IEnumerable<Keys> Chord => _chord;

        /// <summary>
        ///     Number of chord (modifier) keys which must be already down when the trigger key is pressed.
        /// </summary>
        public int ChordLength => _chord.Count;

        //        /// <summary>
        //        ///     A chainable builder method to simplify chord creation. Used along with <see cref="TriggeredBy" />,
        //        ///     <see cref="With" />, <see cref="Control" />, <see cref="Shift" />, <see cref="Alt" />.
        //        /// </summary>
        //        /// <param name="key"></param>
        //        public static Combination TriggeredBy(Keys key)
        //        {
        //            return new Combination(key, (IEnumerable<Keys>) new Chord(Enumerable.Empty<Keys>()));
        //        }

        /// <summary>
        ///     A chainable builder method to simplify chord creation. Used along with <see cref="TriggeredBy" />,
        ///     <see cref="With" />, <see cref="Control" />, <see cref="Shift" />, <see cref="Alt" />.
        /// </summary>
        /// <param name="key"></param>
        public ICombination With(Keys key)
        {
            return new Combination(TriggerKey, Chord.Concat(Enumerable.Repeat(key, 1)));
        }

        public ICombination With(IEnumerable<Keys> chordKeys)
        {
            return chordKeys.Aggregate(this as ICombination, (c, k) => c.With(k));
        }

        public ISequence Then(ICombination combination)
        {
            return new Sequence(this, combination as Combination);
        }

        public ISequence Then(Keys key)
        {
            return this.Then(new Combination(key));
        }

        public bool IsAnyKey(Keys key)
        {
            if (key == TriggerKey) return true;
            if (Chord.Contains(key)) return true;
            return false;
        }


        /// <summary>
        ///     A chainable builder method to simplify chord creation. Used along with <see cref="TriggeredBy" />,
        ///     <see cref="With" />, <see cref="Control" />, <see cref="Shift" />, <see cref="Alt" />.
        /// </summary>
        public ICombination Control()
        {
            return With(Keys.Control);
        }

        /// <summary>
        ///     A chainable builder method to simplify chord creation. Used along with <see cref="TriggeredBy" />,
        ///     <see cref="With" />, <see cref="Control" />, <see cref="Shift" />, <see cref="Alt" />.
        /// </summary>
        public ICombination Alt()
        {
            return With(Keys.Alt);
        }

        /// <summary>
        ///     A chainable builder method to simplify chord creation. Used along with <see cref="TriggeredBy" />,
        ///     <see cref="With" />, <see cref="Control" />, <see cref="Shift" />, <see cref="Alt" />.
        /// </summary>
        public ICombination Shift()
        {
            return With(Keys.Shift);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join("+", Chord.Concat(Enumerable.Repeat(TriggerKey, 1)));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     TriggeredBy a chord from any string like this 'Alt+Shift+R'.
        ///     Nothe that the trigger key must be the last one.
        /// </summary>
        public static ICombination FromString(string trigger)
        {
            var parts = trigger
                .Split('+')
                .Select(p =>
                {
                    try
                    {
                        return Enum.Parse(typeof(Keys), p);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(
                            $@"Could not Parse the keys;{Environment.NewLine}{e.Message} please use the below string (i.e. Control+Z):{Environment.NewLine} {string.Join(", ", Enum.GetNames(typeof(Keys)))}");
                        throw e;
                    }
                })
                .Cast<Keys>();
            var stack      = new Stack<Keys>(parts);
            var triggerKey = stack.Pop();
            return new Combination(triggerKey, stack);
        }

        /// <inheritdoc />
        protected bool Equals(Combination other)
        {
            return
                TriggerKey == other.TriggerKey
                && Chord.Equals(other.Chord);
        }

        public IEnumerator<ICombination> GetEnumerator()
        {
            return new List<ICombination> {this}.GetEnumerator();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Combination) obj);
        }

        private int _hash = 0;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (_hash != 0) return _hash;
            _hash = Chord.GetHashCode() ^ (int) TriggerKey;
            return _hash;
        }
    }
}