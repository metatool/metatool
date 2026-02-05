using System.Windows;
using System.Windows.Input;

namespace Metatool.WebViewHost
{
    public partial class App : Application
    {
        private const int HOTKEY_ID = 9000;
        private HotkeyHandler? _hotkeyHandler;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new WebViewHost();
            MainWindow.Loaded += MainWindow_Loaded;
            MainWindow.Show();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _hotkeyHandler = new HotkeyHandler(MainWindow, HOTKEY_ID, ModifierKeys.Control | ModifierKeys.Shift, Key.F);
            _hotkeyHandler.HotkeyPressed += () => ((WebViewHost)MainWindow).ShowSearch();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hotkeyHandler?.Dispose();
            base.OnExit(e);
        }
    }
}
