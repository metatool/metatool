using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Metatool.MetaPlugin;
using Metatool.ScreenHint;

namespace Metatool.ServiceDebugHelp
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
