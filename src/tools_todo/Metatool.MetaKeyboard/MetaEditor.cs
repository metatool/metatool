using System;
using System.Collections.Generic;
using System.Linq;
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
            (Space + LAlt).Handled(KeyEvent.All).OnHit(async e => { await GoToChar(clipboard, keyboard, e, false); });
            (Caps + J).OnDown(e => // Join lines
            {
                e.Handled = true;
                e.DisableVirtualKeyHandlingInThisEvent();

                keyboard.Type(Down, Home, BS, Space, Left);
            });

            // open line below 
            // bug: in vscode, ctrl+alt+enter to replace all have problem to trigger, 
            (Ctrl + Enter).Handled(KeyEvent.All).OnHit(e =>
            {
                new List<Key> {LCtrl, RCtrl}.ForEach(k =>
                {
                    if (e.KeyboardState.IsDown(k))
                    {
                        keyboard.Up(k); //todo: add UpAsync here
                    }
                });
                keyboard.Type(End, Enter);
            });

            // open line above
            (Ctrl + Shift + Enter).Handled(KeyEvent.All).OnHit(e =>
            {
                new List<Key> {LCtrl, RCtrl, LShift, RShift}.ForEach(k =>
                {
                    if (e.KeyboardState.IsDown(k))
                    {
                        keyboard.Up(k); //todo: add UpAsync here
                    }
                });

                keyboard.Type(Up, End, Enter);
            });

            (Caps + C).OnDown(e => // copy line
            {
                e.Handled = true;
                e.DisableVirtualKeyHandlingInThisEvent();
                keyboard.Type(End, Shift + Home, Ctrl + C, Right);
            }, null, "Copy current line");
        }

        private async Task GoToChar(IClipboard clipboard, IKeyboard keyboard, IKeyEventArgs e, bool right = true)
        {
            keyboard.HandleVirtualKey = false;
            var chordKey   = Space;
            var triggerKey = right ? RAlt : LAlt;

            try
            {
                if (e.KeyboardState.IsDown(triggerKey))
                    keyboard.Up(triggerKey); // RAlt is still down in Hit event

                var selectKeys = right ? Shift + End : ShiftKey + Home;
                keyboard.Type(selectKeys);

                var text = await clipboard.CopyTextAsync(100);

                if (text.Length == 0)
                {
                    var select = right ? Shift + PageDown : ShiftKey + PageUp;
                    keyboard.Type(select);

                    text = await clipboard.CopyTextAsync(100);
                    text = text.Replace("\r\n", "\r");
                }

                if (text.Length == 0)
                    return;
                keyboard.Type(right ? Left : Right); // unselect

                keyboard.DisableChord(chordKey);
                var startIndex   = right ? 0 : text.Length - 1;
                var chordHolding = false;
                var lineSearch   = true;
                do
                {
                    var cs               = new CancellationTokenSource();
                    var charToSearchTask = keyboard.KeyPressAsync(true, 5000, null, cs.Token);

                    if (chordHolding)
                    {
                        var chordUpTask = keyboard.KeyUpAsync(false, 5000, chordKey, cs.Token);
                        var i           = Task.WaitAny(charToSearchTask, chordUpTask);
                        if (!(i == 0 && charToSearchTask.IsCompleted))
                        {
                            cs.Cancel();
                            return;
                        }
                    }

                    var charArgs = await charToSearchTask;
                    // if we search the Upper case Shift is pressed
                    new List<Key> {LShift, RShift}.ForEach(k =>
                    {
                        if (charArgs.EventArgs.KeyboardState.IsDown(k))
                        {
                            keyboard.Up(k); //todo: add UpAsync here
                        }
                    });

                    if (charToSearchTask.IsCompleted)
                    {
                        chordHolding = true;
                        var cha                           = charArgs.KeyChar.ToString();
                        if (charArgs.KeyChar == '\b') cha = " ";
                        if (charArgs.KeyChar == '\r') cha = "\r\r";
                        var index                         = Search(cha, text, startIndex, right);
                        if (index == -1)
                        {
                            if (lineSearch)
                            {
                                lineSearch = false;
                                keyboard.Type(right ? End : Home);
                            }

                            //Thread.Sleep(3);
                            var select = right ? Shift + PageDown : ShiftKey + PageUp;
                            keyboard.Type(select);

                            text = await clipboard.CopyTextAsync(100);
                            text = text.Replace("\r\n", "\r");

                            if (text.Length == 0)
                                return;
                            keyboard.Type(right ? PageUp : PageDown); // unselect
                            startIndex = right ? 0 : text.Length - 1;

                            index = Search(cha, text, startIndex, right);
                        }

                        if (index != -1)
                        {
                            var count = Math.Abs(index - startIndex);
                            count++;
                            var moveKey = right ? Right : Left;
                            for (var j = 0; j < count; j++)
                            {
                                keyboard.Type(moveKey);
                            }

                            startIndex += (right ? count : (-count));
                        }
                    }
                } while (keyboard.State.IsDown(chordKey));
            }
            finally
            {
                keyboard.HandleVirtualKey = true;
                            Console.WriteLine(" 3");
                keyboard.EnableChord(chordKey);
            }
        }

        int Search(string ch, string text, int startIndex = 0, bool right = true)
        {
            if (startIndex >= text.Length || startIndex < 0) return -1;

            var isUpCase = ch.Any(char.IsUpper);
            var option   = isUpCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
            var index = right
                ? text.IndexOf(ch, startIndex, option)
                : text.LastIndexOf(ch, startIndex, option);
            return index;
        }
    }
}