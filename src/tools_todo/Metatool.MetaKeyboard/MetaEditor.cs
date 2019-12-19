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
            (Space + RCtrl).OnHit(async e =>
            {
                keyboard.HandleVirtualKey = false;
                
                try
                {
                    if (e.KeyboardState.IsDown(RCtrl)) keyboard.Up(RCtrl);
                    keyboard.Type(Shift + End);
                    var text = await clipboard.CopyTextAsync();
                    if (text.Length == 0)
                        return;
                    keyboard.Type(Left);
                    var ind = 0;
                    keyboard.DisableDownEvent = true;
                    keyboard.DisableUpEvent = true;
                    do
                    {
                        var charArgs = await keyboard.KeyPressAsync(true);

                        var index = IntelSearch(charArgs.KeyChar, text, ind);
                        if (index != -1)
                        {
                            if (e.KeyboardState.IsDown(LShift)) keyboard.Up(LShift);
                            if (e.KeyboardState.IsDown(RShift)) keyboard.Up(RShift);
                            var count = index - ind;

                            for (var j = 0; j < count; j++)
                            {
                                keyboard.Type(Right);
                            }

                            ind += count;
                        }
                    } while (keyboard.State.IsDown(Space));
                }
                finally
                {
                    keyboard.HandleVirtualKey = true;
                    keyboard.DisableDownEvent = false;
                    keyboard.DisableUpEvent   = false;
                }
            });

            (Caps + J).OnDown(e => // Join lines
            {
                e.Handled = true;
                e.DisableVirtualKeyHandlingInThisEvent();

                keyboard.Type(Down, Home, BS, Space, Left);
            });
            (Caps + C).OnDown(e => // copy line
            {
                e.Handled = true;
                e.DisableVirtualKeyHandlingInThisEvent();
                keyboard.Type(End, Shift + Home, Ctrl + C);
            }, null, "Copy current line");
        }

        int IntelSearch(char ch, string text, int startIndex = 0)
        {
            if (startIndex >= text.Length) return -1;
            var isUpCase = char.IsUpper(ch);
            for (var i = startIndex; i < text.Length; i++)
            {
                if (isUpCase)
                {
                    var chT = text[i];
                    if (ch == chT) return i+1;
                }
                else
                {
                    var chT = char.ToLower(text[i]);
                    if (ch == chT) return i+1;
                }
            }

            return -1;
        }
    }
}