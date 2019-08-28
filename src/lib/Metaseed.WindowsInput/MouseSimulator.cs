using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Metaseed.WindowsInput.Native;

namespace Metaseed.WindowsInput
{
    /// <summary>
    /// Implements the <see cref="IMouseSimulator"/> interface by calling the an <see cref="IInputMessageDispatcher"/> to simulate Mouse gestures.
    /// </summary>
    public class MouseSimulator : IMouseSimulator
    {
        private const int MouseWheelClickSize = 120;

        private readonly IInputSimulator _inputSimulator;

        /// <summary>
        /// The instance of the <see cref="IInputMessageDispatcher"/> to use for dispatching <see cref="INPUT"/> messages.
        /// </summary>
        private readonly IInputMessageDispatcher _messageDispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseSimulator"/> class using an instance of a <see cref="WindowsInputMessageDispatcher"/> for dispatching <see cref="INPUT"/> messages.
        /// </summary>
        /// <param name="inputSimulator">The <see cref="IInputSimulator"/> that owns this instance.</param>
        public MouseSimulator(IInputSimulator inputSimulator)
        {
            if (inputSimulator == null) throw new ArgumentNullException("inputSimulator");

            _inputSimulator = inputSimulator;
            _messageDispatcher = new WindowsInputMessageDispatcher();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseSimulator"/> class using the specified <see cref="IInputMessageDispatcher"/> for dispatching <see cref="INPUT"/> messages.
        /// </summary>
        /// <param name="inputSimulator">The <see cref="IInputSimulator"/> that owns this instance.</param>
        /// <param name="messageDispatcher">The <see cref="IInputMessageDispatcher"/> to use for dispatching <see cref="INPUT"/> messages.</param>
        /// <exception cref="InvalidOperationException">If null is passed as the <paramref name="messageDispatcher"/>.</exception>
        internal MouseSimulator(IInputSimulator inputSimulator, IInputMessageDispatcher messageDispatcher)
        {
            if (inputSimulator == null)
                throw new ArgumentNullException("inputSimulator");

            if (messageDispatcher == null)
                throw new InvalidOperationException(
                    string.Format("The {0} cannot operate with a null {1}. Please provide a valid {1} instance to use for dispatching {2} messages.",
                    typeof(MouseSimulator).Name, typeof(IInputMessageDispatcher).Name, typeof(INPUT).Name));

            _inputSimulator = inputSimulator;
            _messageDispatcher = messageDispatcher;
        }

        /// <summary>
        /// Gets the <see cref="IKeyboardSimulator"/> instance for simulating Keyboard input.
        /// </summary>
        /// <value>The <see cref="IKeyboardSimulator"/> instance.</value>
        public IKeyboardSimulator Keyboard => _inputSimulator.Keyboard;

        /// <summary>
        /// Sends the list of <see cref="INPUT"/> messages using the <see cref="IInputMessageDispatcher"/> instance.
        /// </summary>
        /// <param name="inputList">The <see cref="System.Array"/> of <see cref="INPUT"/> messages to send.</param>
        private void SendSimulatedInput(INPUT[] inputList)
        {
            _messageDispatcher.DispatchInput(inputList);
        }

        /// <summary>
        /// Simulates mouse movement by the specified distance measured as a delta from the current mouse location in pixels.
        /// </summary>
        /// <param name="pixelDeltaX">The distance in pixels to move the mouse horizontally.</param>
        /// <param name="pixelDeltaY">The distance in pixels to move the mouse vertically.</param>
        public IMouseSimulator MoveBy(int pixelDeltaX, int pixelDeltaY)
        {
            var inputList = new InputBuilder().AddRelativeMouseMovement(pixelDeltaX, pixelDeltaY).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Normalizes the coordinates to get the absolute values from 0 to 65536
        /// </summary>
        private static void NormalizeCoordinates(ref double x, ref double y)
        {
            var vScreenWidth  = NativeMethods.GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN);
            var vScreenHeight = NativeMethods.GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN);
            var vScreenLeft   = NativeMethods.GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN);
            var vScreenTop    = NativeMethods.GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN);

            x = (x - vScreenLeft) * 65536 / vScreenWidth  + 65536d / (vScreenWidth  * 2);
            y = (y - vScreenTop)  * 65536 / vScreenHeight + 65536d / (vScreenHeight * 2);
        }
        /// <summary>
        /// Simulates mouse movement to the specified location on the primary display device.
        /// Note: this function not as precise as Position, because the position is calculated relative to 65535
        /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput#remarks
        /// </summary>
        /// <param name="absoluteX">The destination's absolute X-coordinate</param>
        /// <param name="absoluteY">The destination's absolute Y-coordinate</param>
        public IMouseSimulator MoveTo(double absoluteX, double absoluteY)
        {
            NormalizeCoordinates(ref absoluteX, ref absoluteY);
            var inputList = new InputBuilder().AddAbsoluteMouseMovement((int)Math.Truncate(absoluteX), (int)Math.Truncate(absoluteY)).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Flag to indicate if the buttons are swapped (left-handed)
        /// </summary>
        public bool AreButtonsSwapped => InputBuilder.AreButtonsSwapped;
        /// <summary>
        /// Current position of the mouse cursor
        /// </summary>
        public Point Position
        {
            get
            {
                NativeMethods.GetCursorPos(out var point);
                return new Point(point.X, point.Y);
            }
            set => NativeMethods.SetCursorPos(value.X, value.Y);
        }

        /// <summary>
        /// Moves the mouse by a given delta from the current position
        /// </summary>
        /// <param name="deltaX">The delta for the x-axis</param>
        /// <param name="deltaY">The delta for the y-axis</param>
        public IMouseSimulator MoveByWithTrace(int deltaX, int deltaY)
        {
            var currPos = Position;
            return MoveToWithTrace(currPos.X + deltaX, currPos.Y + deltaY);
        }
        /// <summary>
        /// Moves the mouse to a new position
        /// </summary>
        /// <param name="newPosition">The new position for the mouse</param>
        public IMouseSimulator MoveToWithTrace(Point newPosition)
        {
            return MoveToWithTrace(newPosition.X, newPosition.Y);
        }
        /// <summary>
        /// Moves the mouse to a new position
        /// </summary>
        /// <param name="newX">The new position on the x-axis</param>
        /// <param name="newY">The new position on the y-axis</param>
        public IMouseSimulator MoveToWithTrace(int newX, int newY)
        {
            // Get starting position
            var startPos = Position;
            var startX = startPos.X;
            var startY = startPos.Y;

            // Prepare variables
            var totalDistance = startPos.Distance(newX, newY);

            // Calculate the duration for the speed
            var optimalPixelsPerMillisecond = 1;
            var minDuration = 200;
            var maxDuration = 500;
            var duration = Convert.ToInt32(totalDistance / optimalPixelsPerMillisecond).Clamp(minDuration, maxDuration);

            // Calculate the steps for the smoothness
            var optimalPixelsPerStep = 10;
            var minSteps = 10;
            var maxSteps = 50;
            var steps = Convert.ToInt32(totalDistance / optimalPixelsPerStep).Clamp(minSteps, maxSteps);

            // Calculate the interval and the step size
            var interval = duration / steps;
            var stepX = (double)(newX - startX) / steps;
            var stepY = (double)(newY - startY) / steps;

            // Build a list of movement points (except the last one, to set that one perfectly)
            var movements = new List<Point>();
            for (var i = 1; i < steps; i++)
            {
                var tempX = startX + i * stepX;
                var tempY = startY + i * stepY;
                movements.Add(new Point(tempX.ToInt(), tempY.ToInt()));
            }

            // Add an exact point for the last one, if it does not fit exactly
            var lastPoint = movements.Last();
            if (lastPoint.X != newX || lastPoint.Y != newY)
            {
                movements.Add(new Point(newX, newY));
            }

            // Loop thru the steps and set them
            foreach (var point in movements)
            {
                Position = point;
                Thread.Sleep(interval);
            }
            Wait.UntilInputIsProcessed();
            return this;
        }
        /// <summary>
        /// Simulates mouse movement to the specified location on the Virtual Desktop which includes all active displays.
        /// </summary>
        /// <param name="absoluteX">The destination's absolute X-coordinate on the virtual desktop</param>
        /// <param name="absoluteY">The destination's absolute Y-coordinate on the virtual desktop</param>
        public IMouseSimulator MoveMouseToPositionOnVirtualDesktop(double absoluteX, double absoluteY)
        {
            NormalizeCoordinates(ref absoluteX, ref absoluteX);
            var inputList = new InputBuilder().AddAbsoluteMouseMovementOnVirtualDesktop((int)Math.Truncate(absoluteX), (int)Math.Truncate(absoluteY)).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse left button down gesture.
        /// </summary>
        public IMouseSimulator LeftDown()
        {
            var inputList = new InputBuilder().AddMouseButtonDown(MouseButton.LeftButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse left button up gesture.
        /// </summary>
        public IMouseSimulator LeftUp()
        {
            var inputList = new InputBuilder().AddMouseButtonUp(MouseButton.LeftButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse left-click gesture.
        /// </summary>
        public IMouseSimulator LeftClick()
        {
            var inputList = new InputBuilder().AddMouseButtonClick(MouseButton.LeftButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse left button double-click gesture.
        /// </summary>
        public IMouseSimulator LeftDoubleClick()
        {
            var inputList = new InputBuilder().AddMouseButtonDoubleClick(MouseButton.LeftButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }
        /// <summary>
        /// Simulates a mouse Middle button down gesture.
        /// </summary>
        public IMouseSimulator MiddleDown()
        {
            var inputList = new InputBuilder().AddMouseButtonDown(MouseButton.MiddleButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse right button down gesture.
        /// </summary>
        public IMouseSimulator RightDown()
        {
            var inputList = new InputBuilder().AddMouseButtonDown(MouseButton.RightButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }
        /// <summary>
        /// Simulates a mouse Middle button up gesture.
        /// </summary>
        public IMouseSimulator MiddleUp()
        {
            var inputList = new InputBuilder().AddMouseButtonUp(MouseButton.MiddleButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }
        /// <summary>
        /// Simulates a mouse right button up gesture.
        /// </summary>
        public IMouseSimulator RightUp()
        {
            var inputList = new InputBuilder().AddMouseButtonUp(MouseButton.RightButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse right button click gesture.
        /// </summary>
        public IMouseSimulator RightClick()
        {
            var inputList = new InputBuilder().AddMouseButtonClick(MouseButton.RightButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse right button double-click gesture.
        /// </summary>
        public IMouseSimulator RightButtonDoubleClick()
        {
            var inputList = new InputBuilder().AddMouseButtonDoubleClick(MouseButton.RightButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse X button down gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        public IMouseSimulator XButtonDown(int buttonId)
        {
            var inputList = new InputBuilder().AddMouseXButtonDown(buttonId).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Sends a mouse down command for the specified mouse button
        /// </summary>
        /// <param name="mouseButton">The mouse button to press</param>
        public  IMouseSimulator Down(MouseButton mouseButton)
        {
            var inputList = new InputBuilder().AddMouseButtonDown(mouseButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }
        public IMouseSimulator Up(MouseButton mouseButton)
        {
            var inputList = new InputBuilder().AddMouseButtonUp(mouseButton).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }
        /// <summary>
        /// Simulates a mouse X button up gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        public IMouseSimulator XButtonUp(int buttonId)
        {
            var inputList = new InputBuilder().AddMouseXButtonUp(buttonId).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse X button click gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        public IMouseSimulator XButtonClick(int buttonId)
        {
            var inputList = new InputBuilder().AddMouseXButtonClick(buttonId).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse X button double-click gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        public IMouseSimulator XButtonDoubleClick(int buttonId)
        {
            var inputList = new InputBuilder().AddMouseXButtonDoubleClick(buttonId).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates mouse vertical wheel scroll gesture.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user.</param>
        public IMouseSimulator VerticalScroll(int scrollAmountInClicks)
        {
            var inputList = new InputBuilder().AddMouseVerticalWheelScroll(scrollAmountInClicks * MouseWheelClickSize).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Simulates a mouse horizontal wheel scroll gesture. Supported by Windows Vista and later.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated to the right; a negative value indicates that the wheel was rotated to the left.</param>
        public IMouseSimulator HorizontalScroll(int scrollAmountInClicks)
        {
            var inputList = new InputBuilder().AddMouseHorizontalWheelScroll(scrollAmountInClicks * MouseWheelClickSize).ToArray();
            SendSimulatedInput(inputList);
            return this;
        }

        /// <summary>
        /// Drags the mouse horizontally
        /// </summary>
        /// <param name="mouseButton">The mouse button to use for dragging</param>
        /// <param name="startingPoint">Starting point of the drag</param>
        /// <param name="distance">The distance to drag, + for right, - for left</param>
        public IMouseSimulator DragHorizontally(MouseButton mouseButton, Point startingPoint, int distance)
        {
            return Drag(mouseButton, startingPoint, distance, 0);
        }

        /// <summary>
        /// Drags the mouse vertically
        /// </summary>
        /// <param name="mouseButton">The mouse button to use for dragging</param>
        /// <param name="startingPoint">Starting point of the drag</param>
        /// <param name="distance">The distance to drag, + for down, - for up</param>
        public IMouseSimulator  DragVertically(MouseButton mouseButton, Point startingPoint, int distance)
        {
            return Drag(mouseButton, startingPoint, 0, distance);
        }

        /// <summary>
        /// Drags the mouse from the starting point with the given distance.
        /// </summary>
        /// <param name="mouseButton">The mouse button to use for dragging.</param>
        /// <param name="startingPoint">Starting point of the drag.</param>
        /// <param name="distanceX">The x distance to drag, + for down, - for up.</param>
        /// <param name="distanceY">The y distance to drag, + for right, - for left.</param>
        public IMouseSimulator  Drag(MouseButton mouseButton, Point startingPoint, int distanceX, int distanceY)
        {
            var endingPoint = new Point(startingPoint.X + distanceX, startingPoint.Y + distanceY);
            return Drag(mouseButton, startingPoint, endingPoint);
        }

        /// <summary>
        /// Drags the mouse from the starting point to another point.
        /// </summary>
        /// <param name="mouseButton">The mouse button to use for dragging.</param>
        /// <param name="startingPoint">Starting point of the drag.</param>
        /// <param name="endingPoint">Ending point of the drag.</param>
        public IMouseSimulator  Drag(MouseButton mouseButton, Point startingPoint, Point endingPoint)
        {
            Position = startingPoint;
            Wait.UntilInputIsProcessed();
            Down(mouseButton);
            Wait.UntilInputIsProcessed();
            Position = endingPoint;
            Wait.UntilInputIsProcessed();
            Up(mouseButton);
            Wait.UntilInputIsProcessed();
            return this;
        }

        /// <summary>
        /// Sleeps the executing thread to create a pause between simulated inputs.
        /// </summary>
        /// <param name="millsecondsTimeout">The number of milliseconds to wait.</param>
        public IMouseSimulator Sleep(int millsecondsTimeout)
        {
            Thread.Sleep(millsecondsTimeout);
            return this;
        }

        /// <summary>
        /// Sleeps the executing thread to create a pause between simulated inputs.
        /// </summary>
        /// <param name="timeout">The time to wait.</param>
        public IMouseSimulator Sleep(TimeSpan timeout)
        {
            Thread.Sleep(timeout);
            return this;
        }
    }
}