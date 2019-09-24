using System;

namespace Metatool.Input
{
    public static class HotString
     
    {
        public static IMetaKey Map(this string source, string target, Predicate<IKeyEventArgs> predicate = null)
        {
            return Keyboard.Map(source, target, e=> !e.IsVirtual);
        }
    }
}
