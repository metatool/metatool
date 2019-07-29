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

            Keys.Oemtilde.With(Keys.CapsLock).Down(e =>
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
            Keys.K.With(Keys.CapsLock).Map(Keys.Up);
            Keys.J.With(Keys.CapsLock).Map(Keys.Down);
            Keys.H.With(Keys.CapsLock).Map(Keys.Left);
            Keys.L.With(Keys.CapsLock).Map(Keys.Right);
            Keys.I.With(Keys.CapsLock).Map(Keys.Home);
            Keys.O.With(Keys.CapsLock).Map(Keys.End);
            Keys.U.With(Keys.CapsLock).Map(Keys.PageUp);
            Keys.N.With(Keys.CapsLock).Map(Keys.PageDown);

            // LAlt + Move
            Keys.H.With(Keys.LMenu).Map(Keys.Left);
            Keys.J.With(Keys.LMenu).Map(Keys.Down);
            Keys.K.With(Keys.LMenu).Map(Keys.Up);
            Keys.L.With(Keys.LMenu).Map(Keys.Right);
            Keys.I.With(Keys.LMenu).Map(Keys.Home);
            Keys.O.With(Keys.LMenu).Map(Keys.End);
            Keys.U.With(Keys.LMenu).Map(Keys.PageUp);
            Keys.N.With(Keys.LMenu).Map(Keys.PageDown);


            // 
            Keys.Back.With(Keys.CapsLock).Map(Keys.Delete);
            Keys.P.With(Keys.CapsLock).Map(Keys.PrintScreen);
            Keys.B.With(Keys.CapsLock).Map(Keys.Pause); // Break
            Keys.OemSemicolon.With(Keys.CapsLock).Map(Keys.Apps); // like right click on current selection

        }
    }
}