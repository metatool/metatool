namespace Metatool.Plugin
{
    public interface IPlugin
    {
        string Id { get; }
        bool OnLoaded();
        void OnUnloading();
    }
}
