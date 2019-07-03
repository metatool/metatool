using System.Windows.Input;
using Clipboard.ComponentModel.Enums;
using Clipboard.ComponentModel.UI.Controls;
using Clipboard.ViewModels;

namespace Clipboard.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : BlurredWindow
    {
        public SettingsWindow(SettingsViewMode viewMode)
        {
            InitializeComponent();

            var dataContext = (SettingsWindowViewModel)DataContext;
            if (viewMode == SettingsViewMode.None)
            {
                dataContext.ViewMode = SettingsViewMode.General;
            }
            else
            {
                dataContext.ViewMode = viewMode;
            }
        }

        private void DragZoneGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
