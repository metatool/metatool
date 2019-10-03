using Metatool.Command;

namespace Metatool.Input{
    public interface IMeta
    {
        string Name    { get; set; }
        bool   Disable { get; set; }
    }


    public interface IMetaKey : IMeta, IRemove
    {
        IHotkey Hotkey { get; set; }
        IMetaKey ChangeHotkey(IHotkey hotkey);
        void ChangeDescription(string description);
    }

}
