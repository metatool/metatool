using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.MetaPlugin
{
    public abstract class PluginBase: IMetaPlugin
    {
        public virtual bool Init()
        {
            Console.WriteLine($"{this.GetType().Name} loaded.");
            return true;
        }

    }
}
