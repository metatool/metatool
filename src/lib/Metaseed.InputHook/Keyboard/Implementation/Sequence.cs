using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public class Sequence : ISequence
    {
        internal static List<(string str, string toStr)> KeyStringsPairs = new List<(string str, string toStr)>
        {
            ("Caps", "CapsLock"),
            ("Del", "Delete"),
            ("Ins", "Insert"),
            ("Ctrl", "Control"),
            ("LCtrl", "LControl"),
            ("RCtrl", "RControl"),
            ("Backspace", "Back"),
            ("BS", "Back"),
            ("Esc", "Escape"),
        };


        private Gma.System.MouseKeyHook.Sequence _sequence;

        private Sequence(Gma.System.MouseKeyHook.Sequence sequence)
        {
            this._sequence = sequence;
        }

        internal Sequence(params ICombination[] combinations)
        {
            _sequence = Gma.System.MouseKeyHook.Sequence.Of(combinations.Select(c => ((Combination) c)._combination)
                .ToArray());
        }

        public ISequence Then(Keys key)
        {
            return this.Then(new Combination(key));
        }

        public ISequence Then(ICombination combination)
        {
            _sequence = Gma.System.MouseKeyHook.Sequence.Of(_sequence.Append(((Combination) combination)._combination)
                .ToArray());
            return this;
        }

        internal static ISequence FromString(string keys)
        {
            try
            {
                keys = KeyStringsPairs.Aggregate(keys, (acc, p) => acc.Replace(p.str, p.toStr));
                var sequence = Gma.System.MouseKeyHook.Sequence.FromString(keys);
                return new Sequence(sequence);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    $@"Could not Parse the keys: {Environment.NewLine}{ex.Message}; please use the below string (i.e. Control+Z,B):{Environment.NewLine} {string.Join(", ", Enum.GetNames(typeof(Keys)).ToString())}");
                throw ex;
            }
        }

        public void Hit(Action<KeyEventArgsExt> action)
        {
            KeyboardHook.Sequences.Add(this._sequence, action);
        }

        public override bool Equals(object obj)
        {
            return _sequence.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _sequence.GetHashCode();
        }

        public override string ToString()
        {
            return _sequence.ToString();
        }
    }
}