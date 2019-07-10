using System.Linq;
using System.Windows;
using System.Windows.Input;
using Clipboard.ComponentModel.Enums;
using Clipboard.ComponentModel.Messages;
using Clipboard.Core.Desktop.ComponentModel;
using Clipboard.Shared.Core;
using Clipboard.ViewModels;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Metaseed.MetaKeyboard;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Clipboard.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClipboardManager _clipboardManager;

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<Message>(this, MessageIdentifiers.ShowNotifyIconBalloon, ShowNotifyIconBalloon);

            foreach (var menuItem in NotifyIcon.ContextMenu.Items.OfType<MenuItem>())
            {
                menuItem.DataContext = DataContext;
            }

            Hide();
            CoreHelper.MinimizeFootprint();
            _clipboardManager = new ClipboardManager();

        }

        #endregion

        #region Handled Methods

        /// <summary>
        /// A <see cref="EventToCommand"/> would not work because the window is initialized but never loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_ContextMenuClosed(object sender, System.EventArgs e)
        {
            ((MainWindowViewModel)DataContext).ContextMenuClosedCommand.Execute(null);
        }

        /// <summary>
        /// A <see cref="EventToCommand"/> would not work because the window is initialized but never loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (MainWindowViewModel)DataContext;
            if (e.ChangedButton == MouseButton.Left && viewModel.DisplayBarCommand.CanExecute(null))
            {
                viewModel.DisplayBarCommand.Execute(null);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Display a balloon on the notify icon with the specified message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        private void ShowNotifyIconBalloon(Message message)
        {
            Requires.IsTrue(message.Values.Length == 2);

            NotifyIcon.Visibility = Visibility.Collapsed;
            NotifyIcon.Visibility = Visibility.Visible;

            NotifyIcon.ShowBalloonTip(10000, message.Values[0].ToString(), message.Values[1].ToString(), BalloonTipIcon.Info);
        }

        #endregion
    }
}
