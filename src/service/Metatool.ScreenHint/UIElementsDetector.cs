using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;

namespace Metatool.ScreenPoint;

public class UIElementsDetector : IUIElementsDetector
{
	public (Rect winRect, List<Rect> rects) Detect(IntPtr winHandle)
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
			var all     = winElement.FindAll(TreeScope.Descendants, condition);
			var rects   = new List<Rect>();
			var winRect = winElement.Cached.BoundingRectangle;

			foreach (AutomationElement element in all)
			{
				var rect = element.Cached.BoundingRectangle;
				if (rect.Width < 3 || rect.Height < 3) continue;

				rect.X = rect.X - winRect.X;
				rect.Y = rect.Y - winRect.Y;
				rects.Add(rect);
			}

			return (winRect, rects);
		}
	}
}
