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
            Keys.CapsLock.MapOnHit(Keys.Escape, e => e.ScanCode !=0,false);

            Keys.Oemtilde.With(Keys.CapsLock).Down("toggle caps", "", e =>
            {
                var state = ToggleKeys.CapsLock.State;
                if (state == ToggleKeyState.AlwaysOff) ToggleKeys.CapsLock.AlwaysOn();
                if (state == ToggleKeyState.AlwaysOn) ToggleKeys.CapsLock.AlwaysOff();
                e.Handled = true;
            });

            Keys.H.With(Keys.CapsLock).Map(Keys.Left);
            Keys.J.With(Keys.CapsLock).Map(Keys.Down);
            Keys.K.With(Keys.CapsLock).Map(Keys.Up);
            Keys.L.With(Keys.CapsLock).Map(Keys.Right);
            Keys.I.With(Keys.CapsLock).Map(Keys.Home);
            Keys.O.With(Keys.CapsLock).Map(Keys.End);
            Keys.U.With(Keys.CapsLock).Map(Keys.PageUp);
            Keys.N.With(Keys.CapsLock).Map(Keys.PageDown);

            Keys.H.With(Keys.LMenu).Map(Keys.Left);
            Keys.J.With(Keys.LMenu).Map(Keys.Down);
            Keys.K.With(Keys.LMenu).Map(Keys.Up);
            Keys.L.With(Keys.LMenu).Map(Keys.Right);
            Keys.I.With(Keys.LMenu).Map(Keys.Home);
            Keys.O.With(Keys.LMenu).Map(Keys.End);
            Keys.U.With(Keys.LMenu).Map(Keys.PageUp);
            Keys.N.With(Keys.LMenu).Map(Keys.PageDown);


            Keys.Back.With(Keys.CapsLock).Map(Keys.Delete);
            Keys.P.With(Keys.CapsLock).Map(Keys.PrintScreen);
            Keys.B.With(Keys.CapsLock).Map(Keys.Pause);
            Keys.OemSemicolon.With(Keys.CapsLock).Map(Keys.Apps);
            Keys.OemOpenBrackets.With(Keys.CapsLock).Map(Keys.LButton);
            Keys.OemCloseBrackets.With(Keys.CapsLock).Map(Keys.RButton);

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
        }
    }
}
