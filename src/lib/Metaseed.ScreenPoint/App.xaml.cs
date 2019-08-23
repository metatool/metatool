using System.Windows;
using Metaseed.ScreenPoint;

namespace Metaseed.ScreenHint
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Hint _hint;
        public App()
        {
            _hint = new Hint();
            _hint.Hook();
        }
    }
}
