// This code is distributed under MIT license. 
// Copyright (c) 2010-2018 George Mamaladze
// See license.txt or https://mit-license.org/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;

namespace Metaseed.Input.MouseKeyHook
{
    /// <summary>
    ///     Describes key or key combination sequences. e.g. Control+Z,Z
    /// </summary>
    public class Sequence : SequenceBase<ICombination>, ISequence
    {
        private static char Comma = '��';
        internal Sequence(params ICombination[] combinations) : base(combinations)
        {
        }

        public ISequence Then(Keys key)
        {
            return this.Then(new Combination(key));
        }

        public ISequence Then(ICombination combination)
        {
            this.Append(combination);
            return this;
        }

        ICombination With(Keys chordKey)
        {
        }

        ICombination With(IEnumerable<Keys> keys);

        public void Hit(Action<KeyEventArgsExt> action)
        {

        }

        static string PreProcess(string keys)
        {
            var query = new StringBuilder(keys).Replace(" ", ""); // remove space
            if (query[0] == ',' && query[1] == ',') query[0] = Comma;
            query.Replace(",,,", $",{Comma},")
            .Replace("+,", $"+{Comma}")
            .Replace(",+", $"{Comma}+");
            return query.ToString();
        }
        //        /// <summary>
        //        ///     Creates an instance of sequence object from parameters representing keys or key combinations.
        //        /// </summary>
        //        /// <param name="combinations"></param>
        //        /// <returns></returns>
        //        public static Sequence Of(params Combination[] combinations)
        //        {
        //            return new Sequence(combinations);
        //        }

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
            var keys = PreProcess(sequence);
            var par = keys.Split(',');
            var combinations = par.Select(c => Combination.FromString(c.Replace(Comma, ','))).ToArray();
            return new Sequence(combinations);
        }
    }
}