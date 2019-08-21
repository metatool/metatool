using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace Metaseed.ScreenPoint
{
    public class PointBuilder
    {
        public string Keys = @"ASDFQWERZXCVTGBHJKL;'YUIOPNM,./";

        public string GetKey(int index)
        {
            int len = Keys.Length;
            if (index < len) return Keys[index].ToString();
            if (index < len * len + len)
            {
                int a = index / len;
                int b = index % len;
                return Keys[a].ToString() + Keys[b].ToString();
            }

            if (index < len * len * len + len * len + len)
            {
                int a  = index / (len * len);
                int a1 = index % (len * len);
                int b  = a1    / len;
                int b1 = a1    % len;
                return Keys[a].ToString() + Keys[b].ToString() + Keys[b1].ToString();
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

        public void Run()
        {
            var       h          = UI.Window.CurrentWindowHandle;
            using var automation = new UIA3Automation();
            var       element    = automation.FromHandle(h);
            var       all        = element.FindAll(TreeScope.Descendants, new BoolCondition(true));
            var       eles       = new Dictionary<string, (System.Drawing.Rectangle rect,AutomationElement element)>();

            for (var i=0;i<all.Length;i++ )
            {
                var ele = all[i];
                if (ele.CachedChildren.Length > 0)
                {
                    Console.WriteLine($"overlooked: {element.Name}-{ele.ClassName}-{ele.ToString()}");
                    continue;
                }

                if (!ele.IsEnabled || ele.BoundingRectangle.Width < 3 ||
                    ele.BoundingRectangle.Height                                     < 3) continue;

                //ele.DrawHighlight();
                var key = GetKey(i);
                var rec = ele.BoundingRectangle;
                rec.X = rec.X - element.BoundingRectangle.X;
                rec.Y = rec.Y - element.BoundingRectangle.Y;
                eles.Add(key,(rec,ele));
            }

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
                var canv= new Canvas(){Background = Brushes.Transparent};
                grid.Children.Add(border);
                grid.Children.Add(canv);

                win.Content = grid;

           

            foreach (var e in eles)
            {
                var r = new TextBlock(){Foreground = Brushes.Red, Background = Brushes.Yellow, Text = e.Key};
                Canvas.SetLeft(r, e.Value.rect.Left);
                Canvas.SetTop(r, e.Value.rect.Top);
                canv.Children.Add(r);
            }

win.Show();
        }
    }
}