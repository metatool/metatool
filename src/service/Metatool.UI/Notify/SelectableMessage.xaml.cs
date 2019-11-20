using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Metatool.NotifyIcon;
using Metatool.UI;

namespace Metatool.Utils.Notify
{
    public partial class SelectableMessage : UserControl
    {
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            var listBoxItem = (ListBoxItem) listView
                .ItemContainerGenerator
                .ContainerFromItem(listView.SelectedItem);

            listBoxItem.Focus();
        }

        private bool _isClosing = false;

        #region BalloonText dependency property

        #endregion

        public SelectableMessage()
        {
            InitializeComponent();
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
            listView.SelectedIndex = 0;
            this.KeyDown += (o, e) =>
            {
                var index = e.Key - Key.D0;
                if (index >= 0 && DataContext is ObservableCollection<TipItem> col && index < col.Count)
                {
                    listView.SelectedIndex = index;

                    Confirm(null, null);
                }
                else
                {
                    Close();
                    var virtualkeyCode = KeyInterop.VirtualKeyFromKey(e.Key);
                    CloseViaKey?.Invoke(virtualkeyCode);
                }
            };
        }

        public event Action<int> CloseViaKey;

        public TipItem SelectedItem { get; set; }

        IInputElement element = null;

        private void OnBalloonClosing(object sender, RoutedEventArgs e)
        {
            e.Handled  = true;
            _isClosing = true;
        }

        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon?.CloseBalloon();
        }

        private void grid_MouseEnter(object sender, MouseEventArgs e)
        {
            //if we're already running the fade-out animation, do not interrupt anymore
            //(makes things too complicated for the sample)
            if (_isClosing) return;

            //the tray icon assigned this attached property to simplify access
            var taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon?.ResetBalloonCloseTimer();
        }


        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            Close();
        }

        private void Close()
        {
            if (Parent is Popup pp) pp.IsOpen = false;
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedItem = ((sender as FrameworkElement).DataContext as TipItem);
            Close();
            SelectedItem.Action?.Invoke();
        }

        private void Confirm(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
            SelectedItem.Action?.Invoke();
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
