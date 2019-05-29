using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input;

namespace Metaseed.InputHook
{
    using Input;
    public static class ProcessKeyExtension
    {
        public static void HandleKeys(this IKeyboardEvents source,
            IEnumerable<KeyValuePair<(Keys key, KeyEventType type), Action<Metaseed.Input.KeyEventArgsExt>>> map)
        {
         
            var keysMap = map.GroupBy(e => e.Key.type, e => (e.Key.key, action:e.Value)).ToDictionary(g => g.Key, g => g.ToDictionary(t => t.key, t => t.action));
            {
                var keysDown = keysMap[KeyEventType.Down];
                if (keysDown.Count > 0)
                {
                    source.KeyDown += (sender, args) =>
                    {
                        if (keysDown.TryGetValue(args.KeyCode, out var action))
                        {
                            action(args as KeyEventArgsExt);
                        }
                    };
                }
            }
             
            var keysUp = keysMap[KeyEventType.Up];
            if (keysUp.Count > 0)
            {
                source.KeyUp += (sender, args) =>
                {
                    if (keysUp.TryGetValue(args.KeyCode, out var action))
                    {
                        action(args as KeyEventArgsExt);
                    }
                };
            }
            var keysPress = keysMap[KeyEventType.Press];
            if (keysPress.Count > 0)
            {
                source.KeyPress += (sender, args) =>
                {
                    if (keysPress.TryGetValue((Keys)(args.KeyChar), out var action))
                    {
//                        action(args as KeyPressEventArgsExt);
                    }
                };
            }
        }
    }
}
