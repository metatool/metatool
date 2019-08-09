using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class SequenceExtensions
    {
        public static IMetaKey Down(this ValueTuple<ISequenceUnit, ISequenceUnit> sequence,
            Action<KeyEventArgsExt> action,  string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Down(action,description);
        }
        public static IMetaKey Down(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<KeyEventArgsExt> action,  string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Down(action,  description);
        }
        public static IMetaKey Down(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<KeyEventArgsExt> action,  string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Down(action,  description);
        }

        public static IMetaKey Down(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<KeyEventArgsExt> action,  string description = "")
        {
           return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5).Down(action,  description);
        }

        public static IMetaKey Down(this ISequence sequence,
            Action<KeyEventArgsExt> action,  string description = "", KeyStateMachine stateMachine = null)
        {
            var seq = sequence as Sequence;
            Debug.Assert(seq != null, nameof(sequence) + " != null");
            return Keyboard.Add(seq.ToList<ICombination>(), KeyEvent.Down, new KeyCommand( action){Description = description},stateMachine);
        }

        public static IMetaKey Up(this ValueTuple<ISequenceUnit, ISequenceUnit> sequence,
            Action<KeyEventArgsExt> action,  string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Up(action,  description);
        }
        public static IMetaKey Up(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<KeyEventArgsExt> action,  string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Up(action,  description);
        }
        public static IMetaKey Up(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<KeyEventArgsExt> action,  string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Up(action,  description);
        }

        public static IMetaKey Up(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<KeyEventArgsExt> action,  string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5).Up(action,  description);
        }
        public static IMetaKey Up(this ISequence sequence, Action<KeyEventArgsExt> action, 
            string description = "")
        {
            var seq = sequence as Sequence;
            Debug.Assert(seq != null, nameof(sequence) + " != null");
            return Keyboard.Add(seq.ToList<ICombination>(), KeyEvent.Up, new KeyCommand(  action){Description = description});
        }

        public static Task<KeyEventArgsExt> DownAsync(this ISequence sequence, int timeout = 8888)
        {
            var last = sequence.Last();
            sequence.Down(null);
            return last.DownAsync(timeout);
        }

        public static Task<KeyEventArgsExt> UpAsync(this ISequence sequence, int timeout = 8888)
        {
            var last = sequence.Last();
            sequence.Up(null);
            return last.UpAsync(timeout);
        }


    }
}