using System.Windows;
using System.Windows.Controls;

namespace KeyMouse
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public Canvas HintCanvas => _Canvas;
    }
}
