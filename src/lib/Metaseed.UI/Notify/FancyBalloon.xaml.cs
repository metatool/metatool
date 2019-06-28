using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using Metaseed.NotifyIcon;

namespace Metaseed.UI.Notify
{
    public class Des
    {
        public string pre { get; set; }
        public string bold { get; set; }
        public string post { get; set; }
    }
    public class TipItem
    {
        public string key { get; set; }

        Des _Description;

        public Des description
        {
            get { return _Description; }
        }

        public string Description
        {
            set
            {
                _Description = new Des();
                if (string.IsNullOrEmpty(value)) return;
                var b = value.IndexOf('&');
                if (b == -1) _Description.pre= value;
                var parts = value.Split('&');
                _Description.pre = parts[0];
                if (parts.Length <= 1) return;
                _Description.bold = parts[1].Substring(0, 1);
                if (parts[1].Length <= 1) return;
                _Description.post =parts[1].Substring(1);
            }
        }
    }

    /// <summary>
    /// Interaction logic for FancyBalloon.xaml
    /// </summary>
    public partial class FancyBalloon : UserControl
    {
        private bool isClosing = false;

        #region BalloonText dependency property

        /// <summary>
        /// Description
        /// </summary>
        public static readonly DependencyProperty TipsProperty =
            DependencyProperty.Register("Tips",
                typeof(ObservableCollection<TipItem>),
                typeof(FancyBalloon),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// A property wrapper for the <see cref="BalloonTextProperty"/>
        /// dependency property:<br/>
        /// Description
        /// </summary>
        public ObservableCollection<TipItem> Tips
        {
            get { return (ObservableCollection<TipItem>) GetValue(TipsProperty); }
            set { SetValue(TipsProperty, value); }
        }

        #endregion

        public FancyBalloon()
        {
            InitializeComponent();
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        /// <summary>
        /// By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent"/>
        /// and setting the "Handled" property to true, we suppress the popup
        /// from being closed in order to display the custom fade-out animation.
        /// </summary>
        private void OnBalloonClosing(object sender, RoutedEventArgs e)
        {
            e.Handled = true; //suppresses the popup from being closed immediately
            isClosing = true;
        }


        /// <summary>
        /// Resolves the <see cref="TaskbarIcon"/> that displayed
        /// the balloon and requests a close action.
        /// </summary>
        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //the tray icon assigned this attached property to simplify access
            var taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }

        /// <summary>
        /// If the users hovers over the balloon, we don't close it.
        /// </summary>
        private void grid_MouseEnter(object sender, MouseEventArgs e)
        {
            //if we're already running the fade-out animation, do not interrupt anymore
            //(makes things too complicated for the sample)
            if (isClosing) return;

            //the tray icon assigned this attached property to simplify access
            var taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.ResetBalloonCloseTimer();
        }


        /// <summary>
        /// Closes the popup once the fade-out animation completed.
        /// The animation was triggered in XAML through the attached
        /// BalloonClosing event.
        /// </summary>
        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            var pp = (Popup) Parent;
            pp.IsOpen = false;
        }
    }
}