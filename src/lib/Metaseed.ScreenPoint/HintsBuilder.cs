using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Metaseed.ScreenPoint
{
    public static class Config
    {
        public static string Keys = @"ASDFQWERZXCVTGBHJKLYUIOPNM";
    }

    public class HintsBuilder
    {
        private (Rect winRect, List<Rect> rects) GetPoints(IntPtr winHandle)
        {
            var cacheRequest = new CacheRequest();
            // cacheRequest.Add(AutomationElement.NameProperty);
            // cacheRequest.Add(AutomationElement.ClassNameProperty);
            // cacheRequest.Add(AutomationElement.);
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

        private Dictionary<string, Rect> GetKeyPointPairs(List<Rect> rects, string keys)
        {
            var keyPointPairs = new Dictionary<string, Rect>();

            var count      = rects.Count;
            var keyLen     = keys.Length;
            var dimensions = (int) Math.Ceiling(Math.Log(count, keyLen));

            var lowDimCount     = (int) Math.Pow(keyLen, dimensions - 1);
            var usedInLowDim    = (int) Math.Ceiling(((double) count) / lowDimCount);
            var notUsedInLowDim = lowDimCount - usedInLowDim;

            static string getKeyOfDimension(int index, int dimension, string keys)
            {
                var ii  = index;
                var len = keys.Length;
                var sb  = new StringBuilder();
                do
                {
                    var i = ii % len;
                    sb.Insert(0, keys[i]);
                    ii = ii / len;
                } while (ii > 0);

                var r = sb.ToString();
                return r.PadLeft(dimension, keys[0]);
            }

            string getKey(int index)
            {
                if (index < notUsedInLowDim)
                {
                    return getKeyOfDimension(index + usedInLowDim, dimensions - 1, keys);
                }

                return getKeyOfDimension(index - notUsedInLowDim, dimensions, keys);
            }

            for (var i = 0; i < count; i++)
            {
                var key = getKey(i);
                keyPointPairs.Add(key, rects[i]);
            }

            return keyPointPairs;
        }

        public void CreateHint(ScreenHint.MainWindow window, (Rect windowRect, Dictionary<string, Rect> rects) points)
        {
            var rr = points.windowRect;
            window.Top    = rr.Top;
            window.Left   = rr.Left;
            window.Width  = rr.Width;
            window.Height = rr.Height;
            window.Canvas.Children.Clear();
            foreach (var e in points.rects)
            {
                var r = new TextBlock()
                {
                    Foreground = Brushes.Red,
                    Background = Brushes.Yellow,
                    Text       = e.Key,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(r, e.Value.Left + e.Value.Width  / 2 - 10);
                Canvas.SetTop(r, e.Value.Top   + e.Value.Height / 2 - 10);
                window.Canvas.Children.Add(r);
            }
        }

        public (Rect windowRect, Dictionary<string, Rect> rects) BuildHintPositions(
            ScreenHint.MainWindow window)
        {
            var w = new Stopwatch();
            w.Start();
            var h      = UI.Window.CurrentWindowHandle;
            var points = GetPoints(h);
            var eles   = GetKeyPointPairs(points.rects, Config.Keys);
            var rr     = points.winRect;

               return (rr, eles);
        }
    }
}