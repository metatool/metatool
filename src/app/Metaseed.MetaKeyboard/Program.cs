using System;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Metaseed.Input;
using Metaseed.MetaKeyboard;

namespace ConsoleApp1
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Keyboard.KeyPress += (o, e) => { };

            var keyboard61 = new Keyboard61();
            var fun = new FunctionalKeys();

            var software = new Utilities();

            Keyboard.Hook();
            Application.Run(new ApplicationContext());
        }
    }
}
