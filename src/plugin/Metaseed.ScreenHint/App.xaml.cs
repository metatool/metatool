using System.Windows;
using Metaseed.MetaPlugin;

namespace Metaseed.ScreenHint
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            IMetaPlugin plugin = new ScreenHintPlugin();
            plugin.Init();
        }
    }
}
