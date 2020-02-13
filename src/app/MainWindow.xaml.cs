using System.ComponentModel;
using System.Windows;

namespace Metaseed.Metatool
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.Hide();
            e.Cancel = true;
        }
    }
}
