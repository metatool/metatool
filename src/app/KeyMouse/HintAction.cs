using System.Windows;
using Metatool.WindowsInput;

namespace KeyMouse
{
    public enum HintActionMode
    {
        Click,
        MoveCursor
    }

    public class HintAction
    {
        private readonly InputSimulator _inputSimulator;

        public HintActionMode ActionMode { get; set; } = HintActionMode.Click;

        public HintAction(InputSimulator inputSimulator)
        {
            _inputSimulator = inputSimulator;
        }

        public void Execute(Rect overlayRect, Rect relativeRect)
        {
            double x = overlayRect.Left + relativeRect.X + relativeRect.Width / 2;
            double y = overlayRect.Top + relativeRect.Y + relativeRect.Height / 2;

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)x, (int)y);

            if (ActionMode == HintActionMode.Click)
            {
                System.Threading.Thread.Sleep(50);
                _inputSimulator.Mouse.LeftClick();
            }
        }
    }
}
