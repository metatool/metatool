using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class KeysExtensions
    {
        public static IRemovable Down(this Keys key, Action<KeyEventArgsExt> action, string actionId,
            string simpleDescription, bool isAsync = false)
        {
            return Keyboard.Add(new Combination(key), KeyEvent.Down,
                new KeyAction(actionId, simpleDescription, action));
        }

        public static IRemovable Up(this Keys key, Action<KeyEventArgsExt> action, string actionId,
            string simpleDescription)
        {
            return Keyboard.Add(new Combination(key), KeyEvent.Up, new KeyAction(actionId, simpleDescription, action));
        }

        public static IRemovable Hit(this Keys key, string actionId, string description, Action<KeyEventArgsExt> action,
            Predicate<KeyEventArgsExt> predicate, bool markHandled = false)
        {
            var keyAction   = new KeyAction(actionId, description, action);
            var combination = new Combination(key);
            return Keyboard.Hit(combination, keyAction, predicate, markHandled);
        }

        public static IRemovable Map(this Keys key, Keys target, Predicate<KeyEventArgsExt> predicate = null)
        {
            return Keyboard.Map(new Combination(key), new Combination(target), e => predicate == null || predicate(e));
        }

        public static IRemovable MapOnHit(this Keys key, Keys target, Predicate<KeyEventArgsExt> predicate = null,
            bool allUp = true)
        {
            return Keyboard.MapOnHit(new Combination(key), new Combination(target),
                e => predicate == null || predicate(e), allUp);
        }

        public static IRemovable Map(this Keys key, ICombination target, Predicate<KeyEventArgsExt> predicate = null)
        {
            return Keyboard.Map(new Combination(key), target, e => predicate == null || predicate(e));
        }

        public static ICombination With(this Keys key, Keys chord)
        {
            return new Combination(key, chord);
        }

        public static ICombination Handled(this Keys key)
        {
            return new Combination(key).Handled();
        }

        public static ICombination With(this Keys triggerKey, IEnumerable<Keys> chordsKeys)
        {
            return new Combination(triggerKey, chordsKeys);
        }
    }
}