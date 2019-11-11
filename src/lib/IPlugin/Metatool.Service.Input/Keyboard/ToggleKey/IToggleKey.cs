using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Input
{
    public interface IToggleKey
    {
        ToggleKeyState State { get; }
        void AlwaysOn();
        void AlwaysOff();
        void Off();
        void On();
    } 
}
