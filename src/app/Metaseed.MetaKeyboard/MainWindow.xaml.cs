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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Metaseed.Input;
using Keyboard = Metaseed.Input.Keyboard;

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
            new KeysConverter().ConvertToString(Keys.B);
            Keyboard.Hotkey("Ctrl+M,A").Hit(e => Console.WriteLine($"Hello from sequence hotkey: {e}"));
            Keys.B.Hit(e => Console.WriteLine("sss"));
            Keys.A.With(Keys.ShiftKey).With(Keys.Control).Then(Keys.B).Hit(e =>Console.WriteLine("bbbbbbb"));
            Keys.Z.With(Keys.Escape).Hit(e =>
            {
                Console.WriteLine("esc");
            });
                        Keys.Z.With(Keys.CapsLock).Hit(e =>
            {
                Console.WriteLine("esc");
            });
            Keyboard.Hook();
        }
    }
}
