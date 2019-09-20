using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Metatool.NotifyIcon;

namespace Metatool.UI.Notify
{
    public class Description
    {
        public string Pre  { get; set; }
        public string Bold { get; set; }
        public string Post { get; set; }
    }

    public class TipItem
    {
        public string Key { get; set; }

        Description _description;

        public Description Description => _description;

        public string DescriptionInfo
        {
            set
            {
                _description = new Description();
                if (string.IsNullOrEmpty(value)) return;
                var b                         = value.IndexOf('&');
                if (b == -1) _description.Pre = value;
                var parts                     = value.Split('&');
                _description.Pre = parts[0];
                if (parts.Length <= 1) return;
                _description.Bold = parts[1].Substring(0, 1);
                if (parts[1].Length <= 1) return;
                _description.Post = parts[1].Substring(1);
            }
        }

        public Action Action { get; set; }
    }

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
                }
            };
        }

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
