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
            Metaseed.UI.Window.ShowConsole();
            Keyboard.KeyPress += (o, e) => { };

            var keyboard61 = new Keyboard61();
            var fun = new FunctionalKeys();

            var software = new Utilities();

            Keyboard.Hook();
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
