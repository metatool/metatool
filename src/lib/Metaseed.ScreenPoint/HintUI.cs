using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Metaseed.ScreenHint
{
    public class HintUI
    {
        public static HintUI Inst = new HintUI();

        private HintUI()
        {
        }

        readonly ScreenHint.MainWindow _window = MainWindow.Inst;

        public void Show()
        {
            foreach (var point in _points)
            {
                point.Value.hint.Visibility = Visibility.Visible;
            }

            _window._Canvas.Visibility = System.Windows.Visibility.Visible;

            _window.Show();
        }

        public void Hide()
        {
            _window.Hide();
        }

        public void HideHints()
        {
            _window._Canvas.Visibility = System.Windows.Visibility.Hidden;
        }

        public void HideHint(string key)
        {
            _points.TryGetValue(key, out var ui);
            ui.hint.Visibility = Visibility.Hidden;
        }

        public void ShowHints()
        {
            _window._Canvas.Visibility = System.Windows.Visibility.Visible;
        }

        public void HighLight(Rect rect)
        {
            _window.HighLight(rect);
        }
        Dictionary<string, (Rect rect, TextBlock hint)> _points;
        public void CreateHint((Rect windowRect, Dictionary<string, Rect> rects) points)
        {
            _points = new Dictionary<string, (Rect rect, TextBlock hint)>();
            var w = new Stopwatch();
            w.Start();
            var rr = points.windowRect;
            _window.Top    = rr.Top;
            _window.Left   = rr.Left;
            _window.Width  = rr.Width;
            _window.Height = rr.Height;
            var childrenCount = _window._Canvas.Children.Count;
            var i             = 0;
            foreach (var e in points.rects)
            {
                TextBlock r;
                if (i < childrenCount)
                {
                    r      = _window._Canvas.Children[i] as TextBlock;
                    r.Text = e.Key;
                    Canvas.SetLeft(r, e.Value.Left + e.Value.Width  / 2 - 10);
                    Canvas.SetTop(r, e.Value.Top   + e.Value.Height / 2 - 10);
                    r.Visibility = Visibility.Visible;
                }
                else
                {
                    r = new TextBlock()
                    {
                        Foreground = Brushes.Red,
                        Background = Brushes.Yellow,
                        Text       = e.Key,
                        FontWeight = FontWeights.Bold
                    };
                    Canvas.SetLeft(r, e.Value.Left + e.Value.Width  / 2 - 10);
                    Canvas.SetTop(r, e.Value.Top   + e.Value.Height / 2 - 10);
                    r.Visibility = Visibility.Visible;
                    _window._Canvas.Children.Add(r);
                }

                _points.Add(e.Key,(e.Value, r));
                i++;
            }

            for (int j = points.rects.Count; j < childrenCount; j++)
            {
                _window._Canvas.Children[j].Visibility = Visibility.Hidden;
            }

            Console.WriteLine("CreateHint:" + w.ElapsedMilliseconds);
        }
    }
}