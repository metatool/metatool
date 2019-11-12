using Metatool.Command;

namespace Metatool.Service{
    public interface IMeta
    {
        string Id    { get; set; }
        bool   Disable { get; set; }
    }


    public interface IMetaKey : IMeta, IRemove
    {
        IHotkey Hotkey { get; set; }
        IMetaKey ChangeHotkey(IHotkey hotkey);
        void ChangeDescription(string description);
    }

}
