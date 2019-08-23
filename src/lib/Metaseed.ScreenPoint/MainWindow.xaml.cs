using System.Windows;

namespace Metaseed.ScreenHint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static MainWindow Inst;
        public MainWindow()
        {
            InitializeComponent();
            Hide();
            Inst = this;
        }
    }
}
