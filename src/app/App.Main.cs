using System;
using Metatool.Utils;

namespace Metaseed.Metatool
{
    public partial class App
    {
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [STAThread]
        public static void Main(string[] args)
        {
            ConsoleExt.InitialConsole(true);
            InitServices();
            new ArgumentProcessor().ArgumentsProcess(args);
        }

    }
}