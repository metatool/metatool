using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Input
{
    public class Mouse
    {

        public void LeftClick()
        {
            InputSimu.Inst.Mouse.LeftButtonClick();
        }

        public void RightClick()
        {
            InputSimu.Inst.Mouse.RightButtonClick();
        }
    }
}
