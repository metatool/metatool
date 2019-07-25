using System.Windows;
using ConsoleApp1;
using Metaseed.Input;
using Metaseed.NotifyIcon;

namespace Metaseed.MetaKeyboard
{
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Application.Current.MainWindow = new MainWindow();
            Notify.ShowMessage("MetaKeyboard started!");

            UI.Window.InitialConsole();
            Notify.AddContextMenuItem("Show Log", e =>
            {
                if ((string) e.Header == "Hide Log")
                {
                    e.Header = "Show Log";
                    Metaseed.UI.Window.HideConsole();
                    return;
                }

                e.Header = "Hide Log";
                Metaseed.UI.Window.ShowConsole();
            }, null, true);
            Keyboard.KeyPress += (o, e1) => { };


            var keyboard61 = new Keyboard61();
            var mouse = new Mouse();
            var fun = new FunctionalKeys();

            var software = new Utilities();
            // var c = new ClipboardManager();

            Keyboard.Hook();
        }
    }
}