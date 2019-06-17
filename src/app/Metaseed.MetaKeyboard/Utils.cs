using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Documents;

namespace Metaseed.MetaKeyboard
{
    public class Utils
    {
        public static void Run (string cmd)
        {
            var procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + cmd )
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

            System.Diagnostics.Process proc = new System.Diagnostics.Process {StartInfo = procStartInfo};
            proc.Start();
        }
    }
}
