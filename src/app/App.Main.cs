using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Plugin;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool
{
    public partial class App
    {
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [STAThread]
        public static void Main(string[] args)
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();

        }


    }
}