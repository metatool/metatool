using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public class PointBuilder
    {
        public string GetKey(int index)
        {
            var len = Config.Keys.Length;
            // if (index < len) return Keys[index].ToString();
            if (index < len * len + len)
            {
                var a = index / len;
                var b = index % len;
                return Config.Keys[a].ToString() + Config.Keys[b].ToString();
            }

            if (index < len * len * len + len * len + len)
            {
                var a  = index / (len * len);
                var a1 = index % (len * len);
                var b  = a1    / len;
                var b1 = a1    % len;
                return Config.Keys[a].ToString() + Config.Keys[b].ToString() + Config.Keys[b1].ToString();
            }

            return "";

            // 4
            // var i = 0;
            // var acc = len-1;
            // while (index > acc)
            // {
            //     acc += (int)Math.Pow(len, i++);
            // }
        }

        public (Rect windowRect, Dictionary<string, Rect> rects) Run(
            MainWindow window)
        {
            var w = new Stopwatch();
            w.Start();
            var          h            = UI.Window.CurrentWindowHandle;
            CacheRequest cacheRequest = new CacheRequest();
            // cacheRequest.Add(AutomationElement.NameProperty);
            //cacheRequest.Add(AutomationElement.ClassNameProperty);
            //cacheRequest.Add(AutomationElement.);
            cacheRequest.Add(AutomationElement.BoundingRectangleProperty);
            AutomationElementCollection all;
            AutomationElement win;
            using (cacheRequest.Activate())
            {
                win = AutomationElement.FromHandle(h);
                var condition = new AndCondition(
                    new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                    new PropertyCondition(AutomationElement.IsOffscreenProperty, false)
                );
                all = win.FindAll(TreeScope.Descendants, condition);
            }


            var eles = new Dictionary<string, Rect>();
            Debug.WriteLine("----------");

            Debug.WriteLine(w.ElapsedMilliseconds);
            w.Restart();

            var rr = win.Cached.BoundingRectangle;
            for (var i = 0; i < all.Count; i++)
            {
                var ele = all[i];

                var r = ele.Cached.BoundingRectangle;
                if (r.Width  < 3 ||
                    r.Height < 3) continue;
                var key = GetKey(i);
                r.X = r.X - rr.X;
                r.Y = r.Y - rr.Y;
                eles.Add(key, r);
            }

            var color = System.Drawing.Color.Red;

            window.Top    = rr.Top;
            window.Left   = rr.Left;
            window.Width  = rr.Width;
            window.Height = rr.Height;

            Debug.WriteLine(w.ElapsedMilliseconds);
            w.Restart();
            window.Canvas.Children.Clear();
            foreach (var e in eles)
            {
                var r = new TextBlock()
                {
                    Foreground = Brushes.Red, Background = Brushes.Yellow, Text = e.Key, FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(r, e.Value.Left +e.Value.Width/2 -10);
                Canvas.SetTop(r, e.Value.Top +e.Value.Height/2 -10);
                window.Canvas.Children.Add(r);
            }

            Debug.WriteLine(w.ElapsedMilliseconds);
            w.Restart();
            window.Show();
            Debug.WriteLine(w.ElapsedMilliseconds);
            w.Restart();
            return (rr, eles);
        }
    }
}