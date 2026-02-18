using System.Windows;
using System.Windows.Controls;

namespace KeyMouse
{
    public partial class KeyMouseMainWindow : Window
    {
        public KeyMouseMainWindow()
        {
            InitializeComponent();
        }

        public Canvas HintCanvas => _Canvas;
    }
}
