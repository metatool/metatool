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
        public static void Down(this ValueTuple<ICombination, ICombination> sequence,
            Action<KeyEventArgsExt> action, string actionId = "", string description = "")
        {
            sequence.Item1.Then(sequence.Item2).Down(action,actionId,description);
        }
        public static void Down(this ValueTuple<ICombination, ICombination, ICombination> sequence,
            Action<KeyEventArgsExt> action, string actionId = "", string description = "")
        {
            sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Down(action, actionId, description);
        }
        public static void Down(this ValueTuple<ICombination, ICombination, ICombination,ICombination> sequence,
            Action<KeyEventArgsExt> action, string actionId = "", string description = "")
        {
            sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Down(action, actionId, description);
        }

        public static void Down(this ValueTuple<ICombination, ICombination, ICombination, ICombination, ICombination> sequence,
            Action<KeyEventArgsExt> action, string actionId = "", string description = "")
        {
            sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5).Down(action, actionId, description);
        }

        public static void Down(this ISequence sequence,
            Action<KeyEventArgsExt> action, string actionId = "", string description = "")
        {
            var seq = sequence as Sequence;
            Debug.Assert(seq != null, nameof(sequence) + " != null");
            Keyboard.Add(seq.ToList<ICombination>(), KeyEvent.Down, new KeyAction(actionId, description, action));
        }

        public static void Up(this ISequence sequence, Action<KeyEventArgsExt> action, string actionId = "",
            string description = "")
        {
            var seq = sequence as Sequence;
            Debug.Assert(seq != null, nameof(sequence) + " != null");
            Keyboard.Add(seq.ToList<ICombination>(), KeyEvent.Up, new KeyAction(actionId, description, action));
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