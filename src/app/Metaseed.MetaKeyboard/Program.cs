using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metaseed.Input;
using Metaseed.MetaKeyboard;

namespace ConsoleApp1
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Keyboard.KeyPress += (o, e) => { };

         var keyboard61= new Keyboard61(); 
var fun = new FunctionalKeys();

var software = new Utilities();


            Keys.A.With(Keys.Control).Down("", "", e => Console.WriteLine("sssssssssssssssss"));

            Keyboard.Hook();

            Application.Run(new ApplicationContext());
        }
    }
}
