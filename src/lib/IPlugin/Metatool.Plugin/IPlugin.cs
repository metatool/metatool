namespace Metatool.Plugin
{
    public interface IPlugin
    {
        bool OnLoaded();
        void OnUnloading();
    }
}
