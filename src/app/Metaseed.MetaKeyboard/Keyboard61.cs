using System.Windows.Forms;
using Metaseed.Input;

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
            Keys.D1.With(Keys.CapsLock).Map(Keys.F1);
            Keys.D2.With(Keys.CapsLock).Map(Keys.F2);
            Keys.D3.With(Keys.CapsLock).Map(Keys.F3);
            Keys.D4.With(Keys.CapsLock).Map(Keys.F4);
            Keys.D5.With(Keys.CapsLock).Map(Keys.F5);
            Keys.D6.With(Keys.CapsLock).Map(Keys.F6);
            Keys.D7.With(Keys.CapsLock).Map(Keys.F7);
            Keys.D8.With(Keys.CapsLock).Map(Keys.F8);
            Keys.D9.With(Keys.CapsLock).Map(Keys.F9);
            Keys.D0.With(Keys.CapsLock).Map(Keys.F10);
            Keys.OemMinus.With(Keys.CapsLock).Map(Keys.F11);
            Keys.Oemplus.With(Keys.CapsLock).Map(Keys.F12);

            // Move (Vim)
            Keys.K.With(Keys.CapsLock).Map(Keys.Up);
            Keys.J.With(Keys.CapsLock).Map(Keys.Down);
            Keys.H.With(Keys.CapsLock).Map(Keys.Left);
            Keys.L.With(Keys.CapsLock).Map(Keys.Right);
            Keys.I.With(Keys.CapsLock).Map(Keys.Home);
            Keys.O.With(Keys.CapsLock).Map(Keys.End);
            Keys.U.With(Keys.CapsLock).Map(Keys.PageUp);
            Keys.N.With(Keys.CapsLock).Map(Keys.PageDown);

            // RAlt + Move
            Keys.H.With(Keys.RMenu).Map(Keys.Left);
            Keys.J.With(Keys.RMenu).Map(Keys.Down);
            Keys.K.With(Keys.RMenu).Map(Keys.Up);
            Keys.L.With(Keys.RMenu).Map(Keys.Right);
            Keys.I.With(Keys.RMenu).Map(Keys.Home);
            Keys.O.With(Keys.RMenu).Map(Keys.End);
            Keys.U.With(Keys.RMenu).Map(Keys.PageUp);
            Keys.N.With(Keys.RMenu).Map(Keys.PageDown);


            // 
            Keys.Back.With(Keys.CapsLock).Map(Keys.Delete);
            Keys.P.With(Keys.CapsLock).Map(Keys.PrintScreen);
            Keys.B.With(Keys.CapsLock).Map(Keys.Pause); // Break
            Keys.OemSemicolon.With(Keys.CapsLock).Map(Keys.Apps); // like right click on current selection

        }
    }
}