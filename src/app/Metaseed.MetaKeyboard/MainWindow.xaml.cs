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
            //new KeysConverter().ConvertToString(Keys.B);
            Keyboard.KeyPress +=(o, e) => Console.WriteLine();
//                        Keyboard.Hotkey("Ctrl+M,A").Hit(e => Console.WriteLine($"Hello from sequence hotkey: {e}"));
//                        Keys.B.Down("metaseed.b.down","b down", e => Console.WriteLine("sss"));
//                        Keys.B.Up("metaseed.b.up","b up", e => Console.WriteLine("sss_up"));
//                        Keys.A.With(Keys.ShiftKey).With(Keys.Control).Down("metaseed.shif+ctrl+a", "don",e =>Console.WriteLine("shifth+ctrl+a"));
//                        Keys.Z.With(Keys.ShiftKey).Then(Keys.C).Down("aa","bbbbb",()=>
//                        {
//                            Console.WriteLine("esc");
//                        });


            ToggleKeys.CapsLock.AlwaysOn();
            Keys.CapsLock.Hit("","",e=>
            {

                Keyboard.Send(Keys.Escape);
                e.Handled = true;
            });

            Keys.H.With(Keys.CapsLock).Map(Keys.Left);
            Keys.J.With(Keys.CapsLock).Map(Keys.Down);
            Keys.K.With(Keys.CapsLock).Map(Keys.Left);
            Keys.L.With(Keys.CapsLock).Map(Keys.Right);

            Keys.A.With(Keys.Control).Down("", "", e => Console.WriteLine("sssssssssssssssss"));


            //                        Keys.Z.With(Keys.CapsLock).Down(e =>
            //            {
            //                Console.WriteLine("esc");
            //            });
            Keyboard.Hook();
        }
    }
}
