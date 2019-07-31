using System.Windows.Forms;
using Metaseed.Input;
using static Metaseed.Input.Key;
using static Metaseed.MetaKeyboard.KeyboardConfig;
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
            (GK + D1).Map(F1);
            (GK + D2).Map(F2);
            (GK + D3).Map(F3);
            (GK + D4).Map(F4);
            (GK + D5).Map(F5);
            (GK + D6).Map(F6);
            (GK + D7).Map(F7);
            (GK + D8).Map(F8);
            (GK + D9).Map(F9);
            (GK + D0).Map(F10);
            (GK + Minus).Map(F11);
            (GK + Plus).Map(F12);
            
            // Move (Vim)
            (GK + K).Map(Up);
            (GK + J).Map(Down);
            (GK + H).Map(Left);
            (GK + L).Map(Right);
            (GK + I).Map(Home);
            (GK + O).Map(End);
            (GK + U).Map(PageUp);
            (GK + N).Map(PageDown);
            
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
            (GK + Back).Map(Del);
            (GK + P).Map(PrintScreen);
            (GK + B).Map(Pause);        // Break
            (GK + SemiColon).Map(Apps); // like right click on current selection
        }
    }
}