using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using AndCondition = FlaUI.Core.Conditions.AndCondition;
using AutomationElement = FlaUI.Core.AutomationElements.AutomationElement;
using PropertyCondition = FlaUI.Core.Conditions.PropertyCondition;
using TreeScope = FlaUI.Core.Definitions.TreeScope;

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

        public (System.Windows.Window window, Dictionary<string, (System.Drawing.Rectangle rect, AutomationElement element)> dic) Run()
        {
            var w = new Stopwatch();
            w.Start();
            var       h          = UI.Window.CurrentWindowHandle;
            using var automation = new UIA3Automation();
            var       element    = automation.FromHandle(h);
            var all = element.FindAll(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(automation.PropertyLibrary.Element.IsEnabled, true),
                    new PropertyCondition(automation.PropertyLibrary.Element.IsOffscreen, false)
                ));
            var eles = new Dictionary<string, (System.Drawing.Rectangle rect, AutomationElement element)>();
            Debug.WriteLine(w.ElapsedMilliseconds);
            w.Restart();

            var rr = element.BoundingRectangle;
            for (var i = 0; i < all.Length; i++)
            {
                var ele = all[i];

                var r = ele.BoundingRectangle;
                if (r.Width  < 3 ||
                    r.Height < 3) continue;
                var key = GetKey(i);
                r.X = r.X - rr.X -2;
                r.Y = r.Y - rr.Y -1; 
                eles.Add(key, (r, ele));
            }

            Debug.WriteLine(w.ElapsedMilliseconds);
            w.Restart();
            var color = System.Drawing.Color.Red;

            var win = new System.Windows.Window
            {
                AllowsTransparency = true,
                WindowStyle        = WindowStyle.None,
                Topmost            = true,
                ShowActivated      = false,
                ShowInTaskbar      = false,
                Background         = Brushes.Transparent,
                Top                = element.BoundingRectangle.Top,
                Left               = element.BoundingRectangle.Left,
                Width              = element.BoundingRectangle.Width,
                Height             = element.BoundingRectangle.Height,
            };

            var grid = new System.Windows.Controls.Grid();
            var border = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush =
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B)),
            };
            var canv = new Canvas() {Background = Brushes.Transparent};
            grid.Children.Add(border);
            grid.Children.Add(canv);

            win.Content = grid;

            Debug.WriteLine(w.ElapsedMilliseconds);
            w.Restart();

            foreach (var e in eles)
            {
                var r = new TextBlock() {Foreground = Brushes.Red, Background = Brushes.Yellow, Text = e.Key, FontWeight = FontWeights.Bold};
                Canvas.SetLeft(r, e.Value.rect.Left);
                Canvas.SetTop(r, e.Value.rect.Top);
                canv.Children.Add(r);
            }

            Debug.WriteLine(w.ElapsedMilliseconds);
            w.Restart();
            win.Show();
            Debug.WriteLine(w.ElapsedMilliseconds);
            w.Restart();
            return (win,eles);
        }
    }
}