using WindowsInput;

namespace Metaseed.Input
{
    public static class Mouse
    {

        public static void LeftClick()
        {
            InputSimu.Inst.Mouse.LeftButtonClick();
        }

        public static void RightClick()
        {
            InputSimu.Inst.Mouse.RightButtonClick();
        }

        /// <summary>
        /// Simulates mouse vertical wheel scroll gesture.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user.</param>
        public static void VerticalScroll(int scrollAmountInClicks)
        {
            InputSimu.Inst.Mouse.VerticalScroll(scrollAmountInClicks);
        }

        /// <summary>
        /// Simulates a mouse horizontal wheel scroll gesture. Supported by Windows Vista and later.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated to the right; a negative value indicates that the wheel was rotated to the left.</param>
        public static void HorizontalScroll(int scrollAmountInClicks)
        {
            InputSimu.Inst.Mouse.HorizontalScroll(scrollAmountInClicks);
        }

        /// <summary>
        /// Simulates mouse movement by the specified distance measured as a delta from the current mouse location in pixels.
        /// </summary>
        /// <param name="pixelDeltaX">The distance in pixels to move the mouse horizontally.</param>
        /// <param name="pixelDeltaY">The distance in pixels to move the mouse vertically.</param>
        public static void MoveMouseBy(int pixelDeltaX, int pixelDeltaY)
        {
            InputSimu.Inst.Mouse.MoveMouseBy(pixelDeltaX, pixelDeltaY);
        }

        /// <summary>
        /// Simulates mouse movement to the specified location on the primary display device.
        /// </summary>
        /// <param name="absoluteX">The destination's absolute X-coordinate on the primary display device where 0 is the extreme left hand side of the display device and 65535 is the extreme right hand side of the display device.</param>
        /// <param name="absoluteY">The destination's absolute Y-coordinate on the primary display device where 0 is the top of the display device and 65535 is the bottom of the display device.</param>
        public static void MoveMouseTo(double absoluteX, double absoluteY)
        {
            InputSimu.Inst.Mouse.MoveMouseTo(absoluteX, absoluteY);

        }
    }
}
