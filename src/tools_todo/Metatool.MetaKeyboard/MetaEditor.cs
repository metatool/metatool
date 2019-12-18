using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Service;
using static Metatool.Service.Key;

namespace Metatool.Tools.MetaKeyboard
{
    public class MetaEditor
    {
        public MetaEditor(IClipboard clipboard, IKeyboard keyboard)
        {
            string Text;
            (Space + LCtrl).OnDown(async e =>
            {
                keyboard.Type(Shift+Home);
                Text = await clipboard.CopyTextAsync();
                var charArgs = await keyboard.KeyPressAsync(true);
                // charArgs.t.KeyCode
            });

            (Caps+ J).OnDown(e => // Join lines
            {
                e.Handled = true;
                e.BeginInvoke(()=>keyboard.Type(Down,Home,BS, Space, Left));
            });
            (Caps +Shift + C).OnDown(e => // copy line
            {
                e.Handled = true;
                e.BeginInvoke(()=>keyboard.Type(End, Shift+Home, Ctrl+C));
            });
        }

    }
}
