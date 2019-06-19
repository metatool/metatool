using System;
using System.Diagnostics;
using System.Linq;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public  static class SequenceExtensions
    {
       public static void Down(this ISequence sequence, string actionId, string description, Action action)
        {
            var seq = sequence as Sequence;
            Debug.Assert(seq != null, nameof(sequence) + " != null");
            Keyboard.Add(seq.ToList<ICombination>(),KeyEvent.Down, new KeyAction(actionId,description, e=> action()));
        }
    }
}
