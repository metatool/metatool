using System;
using System.Collections.Generic;
using System.Windows.Automation;
using Metatool.Service;
using Metatool.UIElementsDetector;

namespace Metatool.ScreenPoint;

public class UIElement : IUIElement

{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class WpfUIElementsDetector : IUIElementsDetector
{
    /// <summary>
    /// Detects all visible and enabled UI automation elements within the screen that has the specified window
    /// and returns their bounding rectangles of the active window and the elements inside the window.
    /// Note: all coordinates are relative to the MAIN screen's top-left corner, not the window's top-left corner.
    /// Uses TreeWalker instead of FindAll to gracefully skip elements with broken MSAA/IAccessible providers.
    /// </summary>
    /// <param name="winHandle">The native window handle to inspect.</param>
    public (IUIElement screen, IUIElement winRect, List<IUIElement> elements) Detect(IntPtr winHandle)
    {
        var (screen, winRect) = UIElementsDetector.UIElementsDetector.GetWindowRect(winHandle);

        var winElement = AutomationElement.FromHandle(winHandle);
        var winRec = winElement.Current.BoundingRectangle;
        var winRectE = new UIElement { X = (int)winRec.X, Y = (int)winRec.Y, Width = (int)winRec.Width, Height = (int)winRec.Height };

        var condition = new AndCondition(
            new PropertyCondition(AutomationElement.IsEnabledProperty, true),
            new PropertyCondition(AutomationElement.IsOffscreenProperty, false)
        );
        // there is a known issue that FindAll may throw COMException if some element in the tree has broken MSAA/IAccessible provider, so we use TreeWalker to do a depth first search to avoid this issue, and skip the element with broken provider.
        // var all = winElement.FindAll(TreeScope.Descendants, condition);
        var all = FindAll(winElement, TreeScope.Descendants, condition);
        var rects = new List<IUIElement>();

        foreach (AutomationElement element in all)
        {
            var rec = element.Current.BoundingRectangle;
            if (rec.Width >= 3 && rec.Height >= 3)
                rects.Add(new UIElement { X = (int)rec.X - screen.X, Y = (int)rec.Y - screen.Y, Width = (int)rec.Width, Height = (int)rec.Height });
        }

        return (screen, winRectE, rects);

        // Local replacement for AutomationElement.FindAll that uses TreeWalker
        // to gracefully skip elements with broken MSAA/IAccessible providers.
        List<AutomationElement> FindAll(AutomationElement root, TreeScope scope, Condition cond)
        {
            var walker = new TreeWalker(cond);
            var results = new List<AutomationElement>();
            var stack = new Stack<AutomationElement>();

            Push(() => walker.GetFirstChild(root));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                results.Add(current);

                if (scope == TreeScope.Descendants)
                    Push(() => walker.GetFirstChild(current));
                Push(() => walker.GetNextSibling(current));
            }

            return results;

            void Push(Func<AutomationElement> getElement)
            {
                try
                {
                    var el = getElement();
                    if (el != null) stack.Push(el);
                }
                catch { }
            }
        }
    }
}
