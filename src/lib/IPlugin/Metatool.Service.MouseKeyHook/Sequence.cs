using System;
using System.Linq;
using System.Text;

namespace Metatool.Input{
    /// <summary>
    ///     Describes key or key combination sequences. e.g. Control+Z,Z
    /// </summary>
    public partial class Sequence : SequenceBase<ICombination>, ISequence
    {
        private static char Comma = '，';

        public Sequence(params ICombination[] combinations) : base(combinations)
        {
        }

        public bool Disabled
        {
            get => this.Last()?.Disabled ?? false;
            set
            {
                if (this.Length > 0)
                    this.Last().Disabled = value;
            }
        }

        public object Context { get; set; }

        static string PreProcess(string keys)
        {
            var query  = new StringBuilder(keys).Replace(" ", ""); // remove space
            if (query[0] == ',' && query[1] == ',') query[0] = Comma;
            query.Replace(",,,", $",{Comma},")
                .Replace("+,", $"+{Comma}")
                .Replace(",+", $"{Comma}+");
            return query.ToString();
        }
     
        /// <summary>
        ///     Creates an instance of sequnce object from string.
        ///     The string must contain comma ',' delimited list of strings describing keys or key combinations.
        ///     Examples: 'A,B,C' 'Alt+R,S', 'Shift+R,Alt+K'
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Sequence FromString(string sequence)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            var keys         = PreProcess(sequence);
            var par          = keys.Split(',');
            var combinations = par.Select(c => Combination.FromString(c.Replace(Comma, ','))).ToArray();
            return new Sequence(combinations);
        }
    }
}
