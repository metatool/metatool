using Metatool.Command;
using Metatool.Input;
using Metatool.Service;

namespace Metatool.MetaKeyboard
{
    public class HostStrings : CommandPackage
    {
        public HostStrings()
        {
            RegisterCommands();
        }
        public IKeyCommand Tks = "tks".Map("thank you very much");
        public IKeyCommand hh  = "hh".Map("0哈哈😊");
        public IKeyCommand hh1 = "hh".Map("1😊SSSS");
    }
}
