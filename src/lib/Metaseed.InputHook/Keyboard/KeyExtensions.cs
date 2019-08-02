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
        public static ICombination With(this Key key, Keys chord)
        {
            return new Combination(key, chord);
        }

        public static ICombination With(this Key triggerKey, IEnumerable<Key> chordsKeys)
        {
            return new Combination(triggerKey, chordsKeys);
        }

        public static Task<KeyEventArgsExt> DownAsync(this Key key, int timeout = 8888)
        {
            var comb = new Combination(key);
            Keyboard.Add(comb, KeyEvent.Down, null);
            return comb.DownAsync(timeout);
        }

        public static Task<KeyEventArgsExt> UpAsync(this Key key, int timeout = 8888)
        {
            var comb = new Combination(key);
            Keyboard.Add(comb, KeyEvent.Up, null);
            return comb.UpAsync(timeout);
        }
        public static IRemovable AsChordKey(this Key key)
        {
            return key.MapOnHit(key.ToCombination(), e => !e.IsVirtual, false);
        }
    }
}