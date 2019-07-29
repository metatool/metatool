using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace Metaseed.Input
{


    public partial class Combination
    {
        public static Combination operator +(Combination combA, ICombination combB)
        {
            return new Combination(combB.TriggerKey, combA.AllKeys.Concat(combB.Chord));
        }
        public static Combination operator +(Combination keyA, Key keyB)
        {
            return new Combination(keyB.KeyCode, keyA.AllKeys);
        }
    }
}