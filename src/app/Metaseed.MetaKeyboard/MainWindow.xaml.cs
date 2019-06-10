using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Metaseed.Input;

namespace Metaseed.MetaKeyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
              Keyboard.KeyPress +=(o, e) => {};

            ToggleKeys.NumLock.AlwaysOn();
            ToggleKeys.CapsLock.AlwaysOff();
            //            Keys.CapsLock.Hit("","",e=>
            //            {
            //                if (e.ScanCode == 0) return;
            //
            //                Keyboard.Send(Keys.Escape);
            //                e.Handled = true;
            //            });
            Keys.CapsLock.MapOnHit(Keys.Escape, e => e.ScanCode!=0);

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

            Keys.H.With(Keys.LMenu).Map(Keys.Left);
            Keys.J.With(Keys.LMenu).Map(Keys.Down);
            Keys.K.With(Keys.LMenu).Map(Keys.Up);
            Keys.L.With(Keys.LMenu).Map(Keys.Right);




            Keys.A.With(Keys.Control).Down("", "", e => Console.WriteLine("sssssssssssssssss"));


            //                        Keys.Z.With(Keys.CapsLock).Down(e =>
            //            {
            //                Console.WriteLine("esc");
            //            });
            Keyboard.Hook();
        }
    }
}
