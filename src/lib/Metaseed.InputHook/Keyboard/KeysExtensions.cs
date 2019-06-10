using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class KeysExtensions
    {
        public static IRemovable Down(this Keys key, string actionId, string description, Action<KeyEventArgsExt> action)
        {
            return Keyboard.Add(new Combination(key), new KeyAction(actionId, description, action));
        }

        public static IRemovable Up(this Keys key, string actionId, string description, Action<KeyEventArgsExt> action)
        {
            return Keyboard.Add(new Combination(key, KeyEventType.Up), new KeyAction(actionId, description, action));
        }
        //        public static IRemovable Hit(this Keys key, string actionId, string description, Action<KeyEventArgsExt> action)
        //        {
        //            return Keyboard.Add(new Combination(key, KeyEventType.Hit), new KeyAction(actionId, description, action));
        //        }

        public static IRemovable Map(this Keys key, Keys target, Predicate<KeyEventArgsExt> predicate = null)
        {
            return Keyboard.Map(new Combination(key), new Combination(target), e => predicate == null || predicate(e));
        }
        public static IRemovable MapOnHit(this Keys key, Keys target, Predicate<KeyEventArgsExt> predicate = null)
        {
            return Keyboard.MapOnHit(new Combination(key), new Combination(target), e => predicate == null || predicate(e));
        }
        public static IRemovable Map(this Keys key, ICombination target, Predicate<KeyEventArgsExt> predicate = null)
        {
            return Keyboard.Map(new Combination(key), target, e => predicate == null || predicate(e));
        }
        public static ICombination With(this Keys key, Keys chord)
        {
            return new Combination(key, chord);
        }

        public static ICombination With(this Keys triggerKey, IEnumerable<Keys> chordsKeys)
        {
            return new Combination(triggerKey, chordsKeys);
        }
    }
}