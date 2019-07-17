using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Clipboard.Shared.Logs;
using Clipboard.ViewModels;

namespace Clipboard.Views
{
    /// <summary>
    /// Interaction logic for PasteBarList.xaml
    /// </summary>
    public partial class PasteTips : UserControl
    {
        public PasteTips()
        {
            InitializeComponent();

        }
        private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
        }

        internal PasteTipsViewModel ViewModel => (PasteTipsViewModel) DataContext;
    }
}
