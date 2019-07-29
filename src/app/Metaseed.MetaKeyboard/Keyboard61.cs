using System.Windows.Forms;
using Metaseed.Input;
using static Metaseed.Input.Key;

namespace ConsoleApp1
{
    class Keyboard61
    {
        public Keyboard61()
        {
            ToggleKeys.NumLock.AlwaysOn();
            ToggleKeys.CapsLock.AlwaysOff();

            Keys.CapsLock.MapOnHit(Keys.Escape, e => !e.IsVirtual, false);

            (Caps + Tilde).Down(e =>
            {
                e.Handled = true;
                var state = ToggleKeys.CapsLock.State;
                if (state == ToggleKeyState.AlwaysOff)
                    ToggleKeys.CapsLock.AlwaysOn();
                else if (state == ToggleKeyState.AlwaysOn)
                    ToggleKeys.CapsLock.AlwaysOff();
            }, "Metaseed.ToggleCapsLockKey", "Toggle CapsLock");

            // Fn
            (Caps + D1).Map(F1);
            (Caps + D2).Map(F2);
            (Caps + D3).Map(F3);
            (Caps + D4).Map(F4);
            (Caps + D5).Map(F5);
            (Caps + D6).Map(F6);
            (Caps + D7).Map(F7);
            (Caps + D8).Map(F8);
            (Caps + D9).Map(F9);
            (Caps + D0).Map(F10);
            (Caps + Minus).Map(F11);
            (Caps + Plus).Map(F12);

            // Move (Vim)
            (Caps + K).Map(Up);
            (Caps + J).Map(Down);
            (Caps + H).Map(Left);
            (Caps + L).Map(Right);
            (Caps + I).Map(Home);
            (Caps + O).Map(End);
            (Caps + U).Map(PageUp);
            (Caps + N).Map(PageDown);

            // LAlt + Move
            (LMenu + H).Map(Left);
            (LMenu + J).Map(Down);
            (LMenu + K).Map(Up);
            (LMenu + L).Map(Right);
            (LMenu + I).Map(Home);
            (LMenu + O).Map(End);
            (LMenu + U).Map(PageUp);
            (LMenu + N).Map(PageDown);

            // 
            (Caps + Back).Map(Del);
            (Caps + P).Map(PrintScreen);
            (Caps + B).Map(Pause);        // Break
            (Caps + SemiColon).Map(Apps); // like right click on current selection
        }
    }
}