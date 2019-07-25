using System;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class CombinationExtetions
    {
        public static IRemovable Down(this ICombination combination, Action<KeyEventArgsExt> action, string actionId = "", string description="")
        {
            return Keyboard.Add(combination,KeyEvent.Down, new KeyAction(actionId, description, action));
        }

        public static IRemovable Up(this ICombination combination, Action<KeyEventArgsExt> action, string actionId ="", string description ="")
        {
            return Keyboard.Add(combination, KeyEvent.Up, new KeyAction(actionId, description, action));
        }
        public static IRemovable Hit(this ICombination combination,  Action<KeyEventArgsExt> action, Predicate<KeyEventArgsExt> predicate, string actionId, string description, bool markHandled = true)
        {
            var keyAction = new KeyAction(actionId, description, action);
            return Keyboard.Hit(combination, keyAction, predicate, markHandled);
        }
        public static IRemovable Map(this ICombination key, Keys target, Predicate<KeyEventArgsExt> predicate = null, int repeat=1)
        {
            return Keyboard.Map(key as Combination, new Combination(target), predicate, repeat);
        }
        public static IRemovable Map(this ICombination key, ICombination target, Predicate<KeyEventArgsExt> predicate = null, int repeat =1)
        {
            return Keyboard.Map(key as Combination, target, predicate, repeat);
        }
        public static IRemovable MapOnHit(this ICombination key, Keys target, Predicate<KeyEventArgsExt> predicate = null)
        {
            return Keyboard.MapOnHit(key, new Combination(target), e => predicate == null || predicate(e));
        }

        public static IRemovable MapOnHit(this ICombination key, ICombination target, Predicate<KeyEventArgsExt> predicate = null)
        {
            return Keyboard.MapOnHit(key, target, e => predicate == null || predicate(e));
        }
        public static ICombination Handled(this ICombination combination)
        {
            combination.Down( e => e.Handled = true);
            combination.Up( e => e.Handled = true);
            return combination;
        }
    }
}
