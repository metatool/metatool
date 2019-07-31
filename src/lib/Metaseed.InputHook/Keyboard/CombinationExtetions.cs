using System;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class CombinationExtetions
    {
        public static IRemovable Down(this ISequenceUnit sequenceUnit, Action<KeyEventArgsExt> action, string actionId = "", string description="")
        {
            var combination = sequenceUnit.ToCombination();
            return Keyboard.Add(combination, KeyEvent.Down, new KeyAction(actionId, description, action));
        }

        public static IRemovable Up(this ISequenceUnit sequenceUnit, Action<KeyEventArgsExt> action, string actionId ="", string description ="")
        {
            var combination = sequenceUnit.ToCombination();

            return Keyboard.Add(combination, KeyEvent.Up, new KeyAction(actionId, description, action));
        }
        public static IRemovable Hit(this ISequenceUnit sequenceUnit,  Action<KeyEventArgsExt> action, Predicate<KeyEventArgsExt> predicate, string actionId, string description, bool markHandled = true)
        {
            var combination = sequenceUnit.ToCombination();

            var keyAction = new KeyAction(actionId, description, action);
            return Keyboard.Hit(combination, keyAction, predicate, markHandled);
        }
        public static IRemovable Map(this ISequenceUnit key, Keys target, Predicate<KeyEventArgsExt> predicate = null, int repeat=1)
        {
            return Keyboard.Map(key.ToCombination(), new Combination(target), predicate, repeat);
        }
        public static IRemovable Map(this ISequenceUnit key, Key target, Predicate<KeyEventArgsExt> predicate = null, int repeat = 1)
        {
            return Keyboard.Map(key.ToCombination(), new Combination(target), predicate, repeat);
        }
        public static IRemovable Map(this ISequenceUnit key, ICombination target, Predicate<KeyEventArgsExt> predicate = null, int repeat =1)
        {
            return Keyboard.Map(key.ToCombination(), target, predicate, repeat);
        }
        public static IRemovable MapOnHit(this ISequenceUnit key, Keys target, Predicate<KeyEventArgsExt> predicate = null, bool allUp = true)
        {
            return Keyboard.MapOnHit(key.ToCombination(), new Combination(target), predicate,allUp);
        }

        public static IRemovable MapOnHit(this ISequenceUnit key, ICombination target, Predicate<KeyEventArgsExt> predicate = null, bool allUp = true)
        {
            return Keyboard.MapOnHit(key.ToCombination(), target, predicate, allUp);
        }
        public static ICombination Handled(this ISequenceUnit sequenceUnit)
        {
            sequenceUnit.Down( e => e.Handled = true);
            sequenceUnit.Up( e => e.Handled = true);
            return sequenceUnit.ToCombination();
        }

        public static IRemovable ToChoreKey(this ISequenceUnit key)
        {
            return key.MapOnHit(key.ToCombination(), e => !e.IsVirtual, false);
        }
    }
}
