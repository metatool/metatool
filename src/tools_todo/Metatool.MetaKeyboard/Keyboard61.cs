using System;
using System.Windows.Forms;
using Metatool.Command;
using Metatool.Input;
using static Metatool.Input.Key;
using static Metatool.MetaKeyboard.KeyboardConfig;

namespace ConsoleApp1
{
    partial class Keyboard61 : KeyMetaPackage
    {
        public Keyboard61()
        {
            ToggleKeys.NumLock.AlwaysOn();
            ToggleKeys.CapsLock.AlwaysOff();
            SetupWinLock();
        }

        public IKeyboardCommandToken  Esc = Caps.MapOnHit(Keys.Escape, e => !e.IsVirtual, false);

        public IKeyboardCommandToken  ToggleCaps = (Caps + Tilde).Down(e =>
        {
            e.Handled = true;
            var state = ToggleKeys.CapsLock.State;
            if (state == ToggleKeyState.AlwaysOff)
                ToggleKeys.CapsLock.AlwaysOn();
            else if (state == ToggleKeyState.AlwaysOn)
                ToggleKeys.CapsLock.AlwaysOff();
        }, null, "Toggle CapsLock");

        // Fn
        public IKeyboardCommandToken  F1  = (GK + D1).Map(Key.F1);
        public IKeyboardCommandToken  F2  = (GK + D2).Map(Key.F2);
        public IKeyboardCommandToken  F3  = (GK + D3).Map(Key.F3);
        public IKeyboardCommandToken  F4  = (GK + D4).Map(Key.F4);
        public IKeyboardCommandToken  F5  = (GK + D5).Map(Key.F5);
        public IKeyboardCommandToken  F6  = (GK + D6).Map(Key.F6);
        public IKeyboardCommandToken  F7  = (GK + D7).Map(Key.F7);
        public IKeyboardCommandToken  F8  = (GK + D8).Map(Key.F8);
        public IKeyboardCommandToken  F9  = (GK + D9).Map(Key.F9);
        public IKeyboardCommandToken  F10 = (GK + D0).Map(Key.F10);
        public IKeyboardCommandToken  F11 = (GK + Minus).Map(Key.F11);
        public IKeyboardCommandToken  F12 = (GK + Plus).Map(Key.F12);

        // Move (Vim)
        public IKeyboardCommandToken  Up       = (GK + K).Map(Key.Up);
        public IKeyboardCommandToken  Down     = (GK + J).Map(Key.Down);
        public IKeyboardCommandToken  Left     = (GK + H).Map(Key.Left);
        public IKeyboardCommandToken  Right    = (GK + L).Map(Key.Right);
        public IKeyboardCommandToken  Home     = (GK + I).Map(Key.Home);
        public IKeyboardCommandToken  End      = (GK + O).Map(Key.End);
        public IKeyboardCommandToken  PageUp   = (GK + U).Map(Key.PageUp);
        public IKeyboardCommandToken  PageDown = (GK + N).Map(Key.PageDown);

        // LAlt + Move
        public IKeyboardCommandToken  LAltLeft     = (LMenu + H).Map(Keys.Left);
        public IKeyboardCommandToken  LAltDown     = (LMenu + J).Map(Keys.Down);
        public IKeyboardCommandToken  LAltUp       = (LMenu + K).Map(Keys.Up);
        public IKeyboardCommandToken  LAltRight    = (LMenu + L).Map(Keys.Right);
        public IKeyboardCommandToken  LAltHome     = (LMenu + I).Map(Keys.Home);
        public IKeyboardCommandToken  LAltEnd      = (LMenu + O).Map(Keys.End);
        public IKeyboardCommandToken  LAltPageUp   = (LMenu + U).Map(Keys.PageUp);
        public IKeyboardCommandToken  LAltPageDown = (LMenu + N).Map(Keys.PageDown);

        // 
        public IKeyboardCommandToken  Del         = (GK + Back).Map(Key.Del);
        public IKeyboardCommandToken  PrintScreen = (GK + P).Map(Key.PrintScreen);
        public IKeyboardCommandToken  Pause       = (GK + B).Map(Key.Pause);        // Break
        public IKeyboardCommandToken  Apps        = (GK + SemiColon).Map(Key.Apps); // like right click on current selection
    }
}
