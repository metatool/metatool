using System;
using Metaseed.MetaKey;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.Mime;
using System.Threading;
using Microsoft.Win32;
using Registry = Metaseed.MetaKey.Registry;

namespace Metaseed.MetaKeyboard
{
    public class Test
    {
        static void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                RestartFirefox();
            }
            else if (e.Mode == PowerModes.Suspend)
            {
                Processes.Instance.CloseWindows("firefox");
            }
        }

        static void RestartFirefox()
        {
            Processes.Instance.CloseWindows("firefox");
            Process.Start(new ProcessStartInfo {FileName = GetBrowser(), WindowStyle = ProcessWindowStyle.Maximized});
        }

        static void RunGoogle(string filter = null)
        {
            RunUrlWithSelection("www.google.com/search?q={0} " + filter);
        }

        static void RunUrlWithSelection(string uri)
        {
            Keyboard.Default.Send(Keys.Control | Keys.C);
            Thread.Sleep(50);
            var clipboard = Clipboard.GetText();
            var url       = Uri.EscapeUriString(string.Format(uri, clipboard.Replace("&", " ")));
            var startInfo = new ProcessStartInfo("https://"+url);
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow  = true;
            //MessageBox.Show(url);
            Process.Start(startInfo);
        }

        static string GetBrowser()
        {
            // Find the Registry key name for the default browser
            var browserKeyName =
                Registry.Instance.GetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.html\UserChoice",
                    "Progid");
            // Find the executable command associated with the above Registry key
            var browserFullCommand = Registry.Instance.GetValue(@"HKEY_CLASSES_ROOT\" + browserKeyName + @"\shell\open\command");
            // The above RegRead will return the path and executable name of the brower contained within quotes and optional parameters
            // We only want the text contained inside the first set of quotes which is the path and executable
            // Find the ending quote position (we know the beginning quote is in position 0 so start searching at position 1)
            var doubleQuoteIndex = browserFullCommand.IndexOf('"', 1);
            // Extract and return the path and executable of the browser
            return browserFullCommand.Substring(1, doubleQuoteIndex - 1);
        }

        public static void Run()
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Started...");

            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            Keyboard.Default.RegisterHotkeys(new Hotkeys
            {
                {Keys.Control | Keys.Alt | Keys.C, _ => Process.Start(@"D:\Copy paste.txt")},
                {Keys.Control | Keys.N, _ => Process.Start(@"http://")},
                {Keys.Control | Keys.Alt | Keys.I, _ => RunGoogle()},
                {Keys.Control | Keys.Alt | Keys.M, _ => RunGoogle("site:allmusic.com")},
                {Keys.Control | Keys.Alt | Keys.V, _ => RunGoogle("site:allmovie.com")},
//                {Keys.Control | Keys.Alt | Keys.D, _ => RunUrlWithSelection("dexonline.ro/definitie/{0}")},
                {Keys.Control | Keys.Alt | Keys.F, _ => RestartFirefox()},
                {Keys.Control | Keys.Alt | Keys.W, _ => Process.Start(@"http://www.meteoromania.ro/anm/?lang=ro_ro")},
                {
                    Keys.Control | Keys.Alt | Keys.H,
                    _ => Process.Start(
                        @"http://www.accuweather.com/en/ro/bucuresti/287430/hourly-weather-forecast/287430")
                },
            });

            Keyboard.Default.RegisterHotkey(Keys.Control | Keys.Alt | Keys.S, _ =>
            {
                var result = MessageBox.Show("Sleep?", "Confirmation", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    Processes.Instance.CloseWindows("firefox");
                    Application.SetSuspendState(PowerState.Suspend, force: false,
                        disableWakeEvent: true);
                }
            });
            Console.ReadLine();

        }
    }
}