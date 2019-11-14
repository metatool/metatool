using Metatool.Service;

namespace Metatool.MetaKeyboard
{
    public class HostStrings : CommandPackage
    {
        public HostStrings()
        {
            RegisterCommands();
        }
        public IKeyCommand Tks = "tks".HotString("thank you very much");
        public IKeyCommand hh  = "hh".HotString("0哈哈😊");
        public IKeyCommand hh1 = "hh".HotString("1😊SSSS");
    }
}
