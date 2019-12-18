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

            (Caps+ J).OnDown(e =>
            {
                e.Handled = true;
                //keyboard.Type(Home);
                //keyboard.Type(Down,Home,Backspace);
                Console.WriteLine("lllllllllllllllllllllllllllllllllllllllllllllllllllllll--------------");
            });
        }

    }
}
