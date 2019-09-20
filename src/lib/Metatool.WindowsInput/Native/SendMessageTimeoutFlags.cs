using System;

namespace Metatool.WindowsInput.Native
{
    [Flags]
    public enum SendMessageTimeoutFlags : uint
    {
        SMTO_NORMAL             = 0x0,
        SMTO_BLOCK              = 0x1,
        SMTO_ABORTIFHUNG        = 0x2,
        SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
        SMTO_ERRORONEXIT        = 0x20
    }
}
