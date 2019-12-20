using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Service;
using static Metatool.Service.Key;

namespace Metatool.Tools.MetaKeyboard
{
    public class MetaEditor
    {
        public MetaEditor(IClipboard clipboard, IKeyboard keyboard)
        {
            (Space + RAlt).Handled(KeyEvent.All).OnHit(async e => { await GoToChar(clipboard, keyboard, e); });
            (Space + LAlt).Handled(KeyEvent.All).OnHit(async e => { await GoToChar(clipboard, keyboard, e,false); });
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

        private async Task GoToChar(IClipboard clipboard, IKeyboard keyboard, IKeyEventArgs e, bool right = true)
        {
            keyboard.HandleVirtualKey = false;
            var chordKey = Space;
            try
            {
                var triggerKey = right ? RAlt : LAlt;
                if (e.KeyboardState.IsDown(triggerKey))
                    keyboard.Up(triggerKey); // RAlt is still down in Hit event
                var selectKeys = right ? Shift + End : ShiftKey + Home;
                keyboard.Type(selectKeys);
                var text = await clipboard.CopyTextAsync();
                if (text.Length == 0)
                    return;
                keyboard.Type(right ? Left : Right); // unselect
                keyboard.DisableChord(chordKey);
                var ind = right ? 0 : text.Length - 1;
                var pressed = false;
                do
                {
                    var cs          = new CancellationTokenSource();
                    var pressedTask = keyboard.KeyPressAsync(true, 5000, null, cs.Token);
                    if (pressed)
                    {
                        var i = Task.WaitAny(pressedTask, keyboard.KeyUpAsync(false, 5000, Space));
                        if (!(i == 0 && pressedTask.IsCompleted))
                        {
                            cs.Cancel();
                            return;
                        }
                    }

                    var charArgs = await pressedTask;
                    if (pressedTask.IsCompleted)
                    {
                        pressed = true;
                        var cha                           = charArgs.KeyChar;
                        if (charArgs.KeyChar == '\b') cha = ' ';

                        var index = IntelSearch(cha, text, ind, right);
                        if (index != -1)
                        {
                            // if we search the Upper case Shift is pressed
                            if (e.KeyboardState.IsDown(LShift)) 
                                keyboard.Up(LShift);
                            if (e.KeyboardState.IsDown(RShift)) 
                                keyboard.Up(RShift);
                            var count = Math.Abs(index - ind);
                            count = count + 1;
                            var moveKey = right ? Right : Left;
                            for (var j = 0; j < count; j++)
                            {
                                keyboard.Type(moveKey);
                            }

                            ind += (right?count:(-count));
                        }
                    }
                } while (keyboard.State.IsDown(chordKey));
            }
            finally
            {
                keyboard.HandleVirtualKey = true;
                keyboard.EnableChord(chordKey);
            }
        }

        int IntelSearch(char ch, string text, int startIndex = 0, bool right = true)
        {
            if (startIndex >= text.Length) return -1;

            var isUpCase = char.IsUpper(ch);
            var option   = isUpCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
            var index = right
                ? text.IndexOf(ch.ToString(), startIndex, option)
                : text.LastIndexOf(ch.ToString(), startIndex, option);
            return index;
        }
    }
}