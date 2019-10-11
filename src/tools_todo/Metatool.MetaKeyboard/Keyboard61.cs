using System.Windows.Forms;
using Metatool.Command;
using Metatool.Input;
using Metatool.Plugin;
using static Metatool.Input.Key;
using static Metatool.MetaKeyboard.KeyboardConfig;

namespace ConsoleApp1
{
    partial class Keyboard61 : CommandPackage
    {
        public Keyboard61()
        {
            ToggleKeys.NumLock.AlwaysOn();
            ToggleKeys.CapsLock.AlwaysOff();
            SetupWinLock();
        }

        public IKey  Esc = Caps.MapOnHit(Keys.Escape, e => !e.IsVirtual, false);

        public IKey  ToggleCaps = (Caps + Tilde).Down(e =>
        {
            e.Handled = true;
            var state = ToggleKeys.CapsLock.State;
            if (state == ToggleKeyState.AlwaysOff)
                ToggleKeys.CapsLock.AlwaysOn();
            else if (state == ToggleKeyState.AlwaysOn)
                ToggleKeys.CapsLock.AlwaysOff();
        }, null, "Toggle CapsLock");

        // Fn
        public IKey  F1  = (GK + D1).Map(Key.F1);
        public IKey  F2  = (GK + D2).Map(Key.F2);
        public IKey  F3  = (GK + D3).Map(Key.F3);
        public IKey  F4  = (GK + D4).Map(Key.F4);
        public IKey  F5  = (GK + D5).Map(Key.F5);
        public IKey  F6  = (GK + D6).Map(Key.F6);
        public IKey  F7  = (GK + D7).Map(Key.F7);
        public IKey  F8  = (GK + D8).Map(Key.F8);
        public IKey  F9  = (GK + D9).Map(Key.F9);
        public IKey  F10 = (GK + D0).Map(Key.F10);
        public IKey  F11 = (GK + Minus).Map(Key.F11);
        public IKey  F12 = (GK + Plus).Map(Key.F12);

        // Move (Vim)
        public IKey  Up       = (GK + K).Map(Key.Up);
        public IKey  Down     = (GK + J).Map(Key.Down);
        public IKey  Left     = (GK + H).Map(Key.Left);
        public IKey  Right    = (GK + L).Map(Key.Right);
        public IKey  Home     = (GK + I).Map(Key.Home);
        public IKey  End      = (GK + O).Map(Key.End);
        public IKey  PageUp   = (GK + U).Map(Key.PageUp);
        public IKey  PageDown = (GK + N).Map(Key.PageDown);

        // LAlt + Move
        public IKey  LAltLeft     = (LMenu + H).Map(Keys.Left);
        public IKey  LAltDown     = (LMenu + J).Map(Keys.Down);
        public IKey  LAltUp       = (LMenu + K).Map(Keys.Up);
        public IKey  LAltRight    = (LMenu + L).Map(Keys.Right);
        public IKey  LAltHome     = (LMenu + I).Map(Keys.Home);
        public IKey  LAltEnd      = (LMenu + O).Map(Keys.End);
        public IKey  LAltPageUp   = (LMenu + U).Map(Keys.PageUp);
        public IKey  LAltPageDown = (LMenu + N).Map(Keys.PageDown);

        // 
        public IKey  Del         = (GK + Back).Map(Key.Del);
        public IKey  PrintScreen = (GK + P).Map(Key.PrintScreen);
        public IKey  Pause       = (GK + B).Map(Key.Pause);        // Break
        public IKey  Apps        = (GK + SemiColon).Map(Key.Apps); // like right click on current selection
    }
}
