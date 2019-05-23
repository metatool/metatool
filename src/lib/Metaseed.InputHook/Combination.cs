using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Gma.System.MouseKeyHook;

namespace Metaseed.Input
{
    public interface IKeyEvents
    {
        void Down(Action action);
    }

    public interface ICombination : IKeyEvents
    {
        ICombination With(Keys              chordKey);
        ICombination With(IEnumerable<Keys> keys);
        ISequence    Then(Keys              key);
        ISequence    Then(ICombination      combination);
    }

    public class Combination : ICombination
    {
        internal Gma.System.MouseKeyHook.Combination _combination;

        private Combination(Gma.System.MouseKeyHook.Combination combination)
        {
            _combination = combination;
        }
        internal Combination(Keys triggerKey, Keys chord = Keys.None)
        {
            ;
            _combination = Gma.System.MouseKeyHook.Combination.TriggeredBy(triggerKey);
            if (chord != Keys.None) _combination = _combination.With(chord);
        }

        public ICombination With(Keys chordKey)
        {
            _combination = _combination.With(chordKey);
            return this;
        }

        public ICombination With(IEnumerable<Keys> chordKeys)
        {
            _combination = chordKeys.Aggregate(_combination, (c, k) => c.With(k));
            return this;
        }

        public ISequence Then(ICombination combination)
        {
            return new Sequence(this, combination);
        }

        public ISequence Then(Keys key)
        {
            return this.Then(new Combination(key));
        }

        public void Down(Action action)
        {
            KeyboardHook.Combinations.Add(this._combination, action);
        }

        internal static ICombination FromString(string keys)
        {            
            keys = Sequence.KeyStringsPairs.Aggregate(keys, (acc, p) => acc.Replace(p.str, p.toStr));
            try
            {

                var combination = Gma.System.MouseKeyHook.Combination.FromString(keys);
                return new Combination(combination);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    $@"Could not Parse the keys;{Environment.NewLine}{ex.Message} please use the below string (i.e. Control+Z):{Environment.NewLine} {string.Join(", ",Enum.GetNames(typeof(Keys)))}");
                throw ex;
            }

        }

        public override bool Equals(object obj)
        {
            return _combination.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _combination.GetHashCode();
        }

        public override string ToString()
        {
            return _combination.ToString();
        }
    }
}