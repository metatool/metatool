using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
  
    public static class KeysExtensions
    {

        public static ICombination With(this Keys key, Keys chord)
        {
            return new Combination(key, chord);
        }
        public static ICombination With(this Keys triggerKey, IEnumerable<Keys> chordsKeys)
        {
            return new Combination(triggerKey, chordsKeys);
        }

        public static Task<KeyEventArgsExt> DownAsync(this Keys key, int timeout = 8888)
        {
            var comb = new Combination(key);
            Keyboard.Add(comb, KeyEvent.Down, null);
            return comb.DownAsync(timeout);
        }

        public static Task<KeyEventArgsExt> UpAsync(this Keys key, int timeout = 8888)
        {
            var comb = new Combination(key);
            Keyboard.Add(comb, KeyEvent.Up, null);
            return comb.UpAsync(timeout);
        }

        ///

        public static IRemovable Down(this Keys key, Action<KeyEventArgsExt> action, string actionId,
            string simpleDescription)
        {
            return ((Combination) key).Down(action, actionId, simpleDescription);
        }

        public static IRemovable Up(this Keys key, Action<KeyEventArgsExt> action, string actionId,
            string simpleDescription)
        {
            return ((Combination) key).Up(action, actionId, simpleDescription);
        }

        public static IRemovable Hit(this Keys key, string actionId, string description, Action<KeyEventArgsExt> action,
            Predicate<KeyEventArgsExt> predicate, bool markHandled = true)
        {
            return ((Combination) key).Hit(action, predicate, actionId, description, markHandled);
        }

        public static IRemovable Map(this Keys key, Keys target, Predicate<KeyEventArgsExt> predicate = null,
            int repeat = 1)
        {
            return ((Combination) key).Map(target, predicate, repeat);
        }

        public static IRemovable Map(this Keys key, ICombination target, Predicate<KeyEventArgsExt> predicate = null,
            int repeat = 1)
        {
            return ((Combination)key).Map(target, predicate, repeat);
        }

        public static IRemovable MapOnHit(this Keys key, Keys target, Predicate<KeyEventArgsExt> predicate = null,
            bool allUp = true)
        {
            return ((Combination)key).MapOnHit(target, predicate, allUp);
        }

        public static IRemovable MapOnHit(this Keys key, ICombination target, Predicate<KeyEventArgsExt> predicate = null, bool allUp = true)
        {
            return ((Combination)key).MapOnHit(target, predicate, allUp);
        }

        public static ICombination Handled(this Keys key)
        {
            return ((Combination)key).Handled();
        }

       
    }

   }