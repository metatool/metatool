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
                var charArgs = await keyboard.KeyAsync(true);
                // charArgs.t.KeyCode
            });

            (Tab+ J).OnDown(e =>
            {
                Console.WriteLine("llllllllllllllllllllllll");
                e.Handled = true;
                keyboard.Type(Down,Home,Backspace);
            });
        }

    }
}
