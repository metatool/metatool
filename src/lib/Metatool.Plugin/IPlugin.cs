namespace Metatool.Plugin
{
    public interface IPlugin
    {
        bool Init();
        void OnUnloading();
    }
}
