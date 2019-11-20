using System;

namespace Metatool.Service
{
    [Flags]
    public enum KeyEvent {None =0, All= -1, Down =1 , Up = 2, AllUp= 8 }
}
