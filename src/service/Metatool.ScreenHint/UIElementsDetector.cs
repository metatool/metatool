using System;
using System.Collections.Generic;
using System.Windows;
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
    public (IUIElement winRect, List<IUIElement> elements) Detect(IntPtr winHandle)
    {
        var cacheRequest = new CacheRequest();
        cacheRequest.Add(AutomationElement.BoundingRectangleProperty);
        using (cacheRequest.Activate())
        {
            var winElement = AutomationElement.FromHandle(winHandle);
            var condition = new AndCondition(
                new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                new PropertyCondition(AutomationElement.IsOffscreenProperty, false)
            );
            var all = winElement.FindAll(TreeScope.Descendants, condition);
            var rects = new List<IUIElement>();
            var winRec = winElement.Cached.BoundingRectangle;
            var winRect = new UIElement() { X = (int)winRec.X, Y = (int)winRec.Y, Width = (int)winRec.Width, Height = (int)winRec.Height };

            foreach (AutomationElement element in all)
            {
                var rec = element.Cached.BoundingRectangle;
				var rect = new UIElement() { X = (int)rec.X, Y = (int)rec.Y, Width = (int)rec.Width, Height = (int)rec.Height };
                if (rect.Width < 3 || rect.Height < 3) continue;

                rect.X = rect.X - winRect.X;
                rect.Y = rect.Y - winRect.Y;
                rects.Add(rect);
            }

            return (winRect, rects);
        }
    }
}
