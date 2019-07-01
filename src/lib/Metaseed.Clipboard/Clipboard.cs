using System;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.Win32;
using Cp = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace Metaseed.Clipboard
{
    public class Clipboard {
        public Clipboard()
        {
            if (!Cp.IsHistoryEnabled())
            {
                Console.WriteLine("[+] Turning on clipboard history feature...");
                try
                {
                    var rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Clipboard", true);

                    if (rk == null)
                    {
                        Console.WriteLine("[!] Clipboard history feature not available on target! Target needs to be at least Win10 Build 1809.\n[!] Exiting...\n");
                        return;
                    }
                    rk.SetValue("EnableClipboardHistory", "1", RegistryValueKind.DWord);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public Registor A = new Registor();
        public Registor S = new Registor();
        public Registor D = new Registor();
        public Registor F = new Registor();


    }
}
