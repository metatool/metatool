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
            //KeyboardHook.Keys("Ctrl+E,A").Down(() => Console.WriteLine("dddddddd"));
            //Keys.B.Down(() => Console.WriteLine("sss"));
            //Keys.A.With(Keys.ShiftKey).With(Keys.Control).Then(Keys.B).Down(()=>Console.WriteLine("bbbbbbb"));
            Keys.Z.With(Keys.Escape).Down(()=>
            {
                Console.WriteLine("esc");
            });
                        Keys.Z.With(Keys.CapsLock).Down(()=>
            {
                Console.WriteLine("esc");
            });
            KeyboardHook.Run();
        }
    }
}
