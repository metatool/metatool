using Metatool.Service;

namespace Metatool.MetaKeyboard
{
    public class HotStrings : CommandPackage
    {
        public HotStrings(IConfig<Config> config)
        {
            RegisterCommands();
            var hotStrings = config.CurrentValue.HotStringPackage.HotStrings;
            foreach (var hotStr in hotStrings)
            {
                var key = hotStr.Key;

                foreach (var str in hotStr.Value)
                {
                    key.HotString(str);
                }
            }
        }
    }
}