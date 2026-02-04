using System.Windows;
using System.Windows.Controls;

namespace KeyMouse
{
    public partial class MainWindow : Window
    {
        private static MainWindow _instance;
        public static MainWindow Instance => _instance ?? (_instance = new MainWindow());

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;
        }

        public Canvas HintCanvas => _Canvas;
    }
}
