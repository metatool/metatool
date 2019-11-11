namespace Metatool.Service
{
    public interface IPlugin
    {
        string Id { get; }
        bool OnLoaded();
        void OnUnloading();
    }
}
