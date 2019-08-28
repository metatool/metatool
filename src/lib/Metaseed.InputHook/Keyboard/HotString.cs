using System;

namespace Metaseed.Input
{
    public static class HotString
     
    {
        public static IMetaKey Map(this string source, string target, Predicate<KeyEventArgsExt> predicate = null)
        {
            return Keyboard.Map(source, target, e=> !e.IsVirtual);
        }
    }
}
