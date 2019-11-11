using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Metatool.Service
{
    public interface IMouse
    {
        Point Position { get; set; }
        IMouse LeftClick();
        IMouse MoveByLikeUser(int deltaX, int deltaY);
        IMouse MoveToLikeUser(int x, int y);
        IMouse VerticalScroll(int scrollAmountInClicks);
        IMouse HorizontalScroll(int scrollAmountInClicks);
    }
}
