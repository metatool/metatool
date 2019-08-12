using System.Windows.Forms;
using Metaseed.Input;
using static Metaseed.Input.Key;
using static Metaseed.MetaKeyboard.KeyboardConfig;

namespace ConsoleApp1
{
    class Keyboard61: KeyMetaPackage
    {
        public Keyboard61()
        {
            ToggleKeys.NumLock.AlwaysOn();
            ToggleKeys.CapsLock.AlwaysOff();
        }

        public IMetaKey Esc = Caps.MapOnHit(Keys.Escape, e => !e.IsVirtual, false);

        public IMetaKey ToggleCaps = (Caps + Tilde).Down(e =>
        {
            e.Handled = true;
            var state = ToggleKeys.CapsLock.State;
            if (state == ToggleKeyState.AlwaysOff)
                ToggleKeys.CapsLock.AlwaysOn();
            else if (state == ToggleKeyState.AlwaysOn)
                ToggleKeys.CapsLock.AlwaysOff();
        }, null, "Toggle CapsLock");

        // Fn
        public IMetaKey F1  = (GK + D1).Map(Key.F1);
        public IMetaKey F2  = (GK + D2).Map(Key.F2);
        public IMetaKey F3  = (GK + D3).Map(Key.F3);
        public IMetaKey F4  = (GK + D4).Map(Key.F4);
        public IMetaKey F5  = (GK + D5).Map(Key.F5);
        public IMetaKey F6  = (GK + D6).Map(Key.F6);
        public IMetaKey F7  = (GK + D7).Map(Key.F7);
        public IMetaKey F8  = (GK + D8).Map(Key.F8);
        public IMetaKey F9  = (GK + D9).Map(Key.F9);
        public IMetaKey F10 = (GK + D0).Map(Key.F10);
        public IMetaKey F11 = (GK + Minus).Map(Key.F11);
        public IMetaKey F12 = (GK + Plus).Map(Key.F12);

        // Move (Vim)
        public IMetaKey Up       = (GK + K).Map(Key.Up);
        public IMetaKey Down     = (GK + J).Map(Key.Down);
        public IMetaKey Left     = (GK + H).Map(Key.Left);
        public IMetaKey Right    = (GK + L).Map(Key.Right);
        public IMetaKey Home     = (GK + I).Map(Key.Home);
        public IMetaKey End      = (GK + O).Map(Key.End);
        public IMetaKey PageUp   = (GK + U).Map(Key.PageUp);
        public IMetaKey PageDown = (GK + N).Map(Key.PageDown);

        // LAlt + Move
        public IMetaKey LAltLeft     = (LMenu + H).Map(Keys.Left);
        public IMetaKey LAltDown     = (LMenu + J).Map(Keys.Down);
        public IMetaKey LAltUp       = (LMenu + K).Map(Keys.Up);
        public IMetaKey LAltRight    = (LMenu + L).Map(Keys.Right);
        public IMetaKey LAltHome     = (LMenu + I).Map(Keys.Home);
        public IMetaKey LAltEnd      = (LMenu + O).Map(Keys.End);
        public IMetaKey LAltPageUp   = (LMenu + U).Map(Keys.PageUp);
        public IMetaKey LAltPageDown = (LMenu + N).Map(Keys.PageDown);

        // 
        public IMetaKey Del         = (GK + Back).Map(Key.Del);
        public IMetaKey PrintScreen = (GK + P).Map(Key.PrintScreen);
        public IMetaKey Pause       = (GK + B).Map(Key.Pause);        // Break
        public IMetaKey Apps        = (GK + SemiColon).Map(Key.Apps); // like right click on current selection

    }
}