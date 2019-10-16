using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Metatool
{
    public partial class App
    {
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [STAThread]
        public static void Main(string[] args)
        {
            // ArgumentsProcess(args);
            var application = new App();
            application.InitializeComponent();
            application.Run(); 
        }


    }
}