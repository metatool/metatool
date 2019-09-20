using System;

namespace Metatool.MetaPlugin
{
    public interface IMetaPlugin
    {
        bool Init();
        void OnUnloading();
    }
}
