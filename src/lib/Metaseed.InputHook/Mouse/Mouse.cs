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
    }
}
