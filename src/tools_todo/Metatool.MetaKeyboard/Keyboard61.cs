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
            RegisterCommands();
        }

        public IKeyCommand  Esc = Caps.MapOnHit(Keys.Escape, e => !e.IsVirtual, false);

        public IKeyCommand  ToggleCaps = (Caps + Tilde).Down(e =>
        {
            e.Handled = true;
            var state = ToggleKeys.CapsLock.State;
            if (state == ToggleKeyState.AlwaysOff)
                ToggleKeys.CapsLock.AlwaysOn();
            else if (state == ToggleKeyState.AlwaysOn)
                ToggleKeys.CapsLock.AlwaysOff();
        }, null, "Toggle CapsLock");

        // Fn
        public IKeyCommand  F1  = (GK + D1).Map(Key.F1);
        public IKeyCommand  F2  = (GK + D2).Map(Key.F2);
        public IKeyCommand  F3  = (GK + D3).Map(Key.F3);
        public IKeyCommand  F4  = (GK + D4).Map(Key.F4);
        public IKeyCommand  F5  = (GK + D5).Map(Key.F5);
        public IKeyCommand  F6  = (GK + D6).Map(Key.F6);
        public IKeyCommand  F7  = (GK + D7).Map(Key.F7);
        public IKeyCommand  F8  = (GK + D8).Map(Key.F8);
        public IKeyCommand  F9  = (GK + D9).Map(Key.F9);
        public IKeyCommand  F10 = (GK + D0).Map(Key.F10);
        public IKeyCommand  F11 = (GK + Minus).Map(Key.F11);
        public IKeyCommand  F12 = (GK + Plus).Map(Key.F12);

        // Move (Vim)
        public IKeyCommand  Up       = (GK + K).Map(Key.Up);
        public IKeyCommand  Down     = (GK + J).Map(Key.Down);
        public IKeyCommand  Left     = (GK + H).Map(Key.Left);
        public IKeyCommand  Right    = (GK + L).Map(Key.Right);
        public IKeyCommand  Home     = (GK + I).Map(Key.Home);
        public IKeyCommand  End      = (GK + O).Map(Key.End);
        public IKeyCommand  PageUp   = (GK + U).Map(Key.PageUp);
        public IKeyCommand  PageDown = (GK + N).Map(Key.PageDown);

        // LAlt + Move
        public IKeyCommand  LAltLeft     = (LMenu + H).Map(Keys.Left);
        public IKeyCommand  LAltDown     = (LMenu + J).Map(Keys.Down);
        public IKeyCommand  LAltUp       = (LMenu + K).Map(Keys.Up);
        public IKeyCommand  LAltRight    = (LMenu + L).Map(Keys.Right);
        public IKeyCommand  LAltHome     = (LMenu + I).Map(Keys.Home);
        public IKeyCommand  LAltEnd      = (LMenu + O).Map(Keys.End);
        public IKeyCommand  LAltPageUp   = (LMenu + U).Map(Keys.PageUp);
        public IKeyCommand  LAltPageDown = (LMenu + N).Map(Keys.PageDown);

        // 
        public IKeyCommand  Del         = (GK + Back).Map(Key.Del);
        public IKeyCommand  PrintScreen = (GK + P).Map(Key.PrintScreen);
        public IKeyCommand  Pause       = (GK + B).Map(Key.Pause);        // Break
        public IKeyCommand  Apps        = (GK + SemiColon).Map(Key.Apps); // like right click on current selection
    }
}
