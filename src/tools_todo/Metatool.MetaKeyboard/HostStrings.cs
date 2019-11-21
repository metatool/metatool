using Metatool.Service;

namespace Metatool.MetaKeyboard
{
    public class HotStrings : CommandPackage
    {
        public HotStrings(IConfig<Config> config)
        {
            RegisterCommands();
            var hotStrings = config.CurrentValue.HotStringPackage.HotStrings;
            foreach (var (key, strings) in hotStrings)
            {
                foreach (var str in strings)
                {
                    key.HotString(str);
                }
            }
        }
    }
}