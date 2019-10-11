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

        public IKeyToken  Esc = Caps.MapOnHit(Keys.Escape, e => !e.IsVirtual, false);

        public IKeyToken  ToggleCaps = (Caps + Tilde).Down(e =>
        {
            e.Handled = true;
            var state = ToggleKeys.CapsLock.State;
            if (state == ToggleKeyState.AlwaysOff)
                ToggleKeys.CapsLock.AlwaysOn();
            else if (state == ToggleKeyState.AlwaysOn)
                ToggleKeys.CapsLock.AlwaysOff();
        }, null, "Toggle CapsLock");

        // Fn
        public IKeyToken  F1  = (GK + D1).Map(Key.F1);
        public IKeyToken  F2  = (GK + D2).Map(Key.F2);
        public IKeyToken  F3  = (GK + D3).Map(Key.F3);
        public IKeyToken  F4  = (GK + D4).Map(Key.F4);
        public IKeyToken  F5  = (GK + D5).Map(Key.F5);
        public IKeyToken  F6  = (GK + D6).Map(Key.F6);
        public IKeyToken  F7  = (GK + D7).Map(Key.F7);
        public IKeyToken  F8  = (GK + D8).Map(Key.F8);
        public IKeyToken  F9  = (GK + D9).Map(Key.F9);
        public IKeyToken  F10 = (GK + D0).Map(Key.F10);
        public IKeyToken  F11 = (GK + Minus).Map(Key.F11);
        public IKeyToken  F12 = (GK + Plus).Map(Key.F12);

        // Move (Vim)
        public IKeyToken  Up       = (GK + K).Map(Key.Up);
        public IKeyToken  Down     = (GK + J).Map(Key.Down);
        public IKeyToken  Left     = (GK + H).Map(Key.Left);
        public IKeyToken  Right    = (GK + L).Map(Key.Right);
        public IKeyToken  Home     = (GK + I).Map(Key.Home);
        public IKeyToken  End      = (GK + O).Map(Key.End);
        public IKeyToken  PageUp   = (GK + U).Map(Key.PageUp);
        public IKeyToken  PageDown = (GK + N).Map(Key.PageDown);

        // LAlt + Move
        public IKeyToken  LAltLeft     = (LMenu + H).Map(Keys.Left);
        public IKeyToken  LAltDown     = (LMenu + J).Map(Keys.Down);
        public IKeyToken  LAltUp       = (LMenu + K).Map(Keys.Up);
        public IKeyToken  LAltRight    = (LMenu + L).Map(Keys.Right);
        public IKeyToken  LAltHome     = (LMenu + I).Map(Keys.Home);
        public IKeyToken  LAltEnd      = (LMenu + O).Map(Keys.End);
        public IKeyToken  LAltPageUp   = (LMenu + U).Map(Keys.PageUp);
        public IKeyToken  LAltPageDown = (LMenu + N).Map(Keys.PageDown);

        // 
        public IKeyToken  Del         = (GK + Back).Map(Key.Del);
        public IKeyToken  PrintScreen = (GK + P).Map(Key.PrintScreen);
        public IKeyToken  Pause       = (GK + B).Map(Key.Pause);        // Break
        public IKeyToken  Apps        = (GK + SemiColon).Map(Key.Apps); // like right click on current selection
    }
}
