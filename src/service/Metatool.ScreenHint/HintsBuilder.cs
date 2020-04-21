using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using Metatool.Service;

namespace Metatool.ScreenPoint
{
    public static class Config
    {
        public static string Keys = @"ASDFQWERZXCVTGBHJKLYUIOPNM";
    }

    public class HintsBuilder
    {

        private static IWindowManager _windowManager;
        private static IWindowManager WindowManager=>_windowManager??= Services.Get<IWindowManager>();
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
            var usedInLowDim    =  (int) Math.Ceiling(((double) (count - lowDimCount)) / (dimensions - 1));//(int) Math.Ceiling(((double) count) / lowDimCount);
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


        public (Rect windowRect, Dictionary<string, Rect> rects) BuildHintPositions()
        {
            var w = new Stopwatch();
            w.Start();
            var h      = WindowManager.CurrentWindow.Handle;
            var points = GetPoints(h);
            Console.WriteLine("GetPoints:" + w.ElapsedMilliseconds);
            w.Restart();
            var eles = GetKeyPointPairs(points.rects, Config.Keys);
            Console.WriteLine("GetKeyPointPairs:" + w.ElapsedMilliseconds);
            w.Restart();
            var rr = points.winRect;
            return (rr, eles);
        }
    }
}
