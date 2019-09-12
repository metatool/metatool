using System.Windows.Input;
using Clipboard.ComponentModel.UI.Controls;
using Clipboard.ViewModels;

namespace Clipboard.Views
{
    /// <summary>
    /// Interaction logic for FirstStartWindow.xaml
    /// </summary>
    public partial class FirstStartWindow : BlurredWindow
    {
        public FirstStartWindow()
        {
            InitializeComponent();

            if (((FirstStartWindowViewModel)DataContext).IsMigrationRequired)
            {
                FlipView.Items.Remove(LanguageTab);
                FlipView.Items.Remove(IgnoredAppTab);
                FlipView.Items.Remove(SynchronizationTab);
                FlipView.Items.Remove(TutorialTab);
                FlipView.SelectedIndex = 0;
            }
        }

        private void DragZoneGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
