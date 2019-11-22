using System.Drawing;

namespace Metatool.Service
{
    public interface IMouse
    {
        Point Position { get; set; }
        IMouse LeftClick();
        IMouse RightClick();
        IMouse MoveByLikeUser(int deltaX, int deltaY);
        IMouse MoveToLikeUser(int x, int y);
        IMouse VerticalScroll(int scrollAmountInClicks);
        IMouse HorizontalScroll(int scrollAmountInClicks);
    }
}
