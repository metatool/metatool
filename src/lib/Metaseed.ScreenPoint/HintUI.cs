using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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

        public void Show(bool isReshow = false)
        {
            if (isReshow)
            {
                foreach (var point in _points)
                {
                    point.Value.hint.Visibility = Visibility.Visible;
                }

                foreach (var hint in markedHints)
                {
                    foreach (Run run in hint.Inlines)
                    {
                        run.ClearValue(TextElement.ForegroundProperty);                    }
                }
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
        List<TextBlock> markedHints = new List<TextBlock>();

        public void MarkHit(string key, int len)
        {
            _points.TryGetValue(key, out var ui);
            markedHints.Add(ui.hint);
            var i = 0;
            foreach (Run run in ui.hint.Inlines)
            {
                if (i == len) break;
                run.Foreground = Brushes.Blue;
                i++;
            }
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

            static void SetText(TextBlock ui, string key)
            {
                ui.Inlines.Clear();
                foreach (var c in key)
                {
                    var run = new Run()
                    {
                        Text = c.ToString()
                    };
                    // run.SetValue(UIElement.IsHitTestVisibleProperty,false);
                    ui.Inlines.Add(run);
                }
            }

            foreach (var e in points.rects)
            {
                TextBlock r;
                if (i < childrenCount)
                {
                    r      = _window._Canvas.Children[i] as TextBlock;
                    Canvas.SetLeft(r, e.Value.Left + e.Value.Width  / 2 - 10);
                    Canvas.SetTop(r, e.Value.Top   + e.Value.Height / 2 - 10);
                }
                else
                {
                    r = new TextBlock()
                    {
                        Foreground = Brushes.Red,
                        Background = Brushes.Yellow,
                        FontWeight = FontWeights.Bold,
                    };
                    Canvas.SetLeft(r, e.Value.Left + e.Value.Width  / 2 - 10);
                    Canvas.SetTop(r, e.Value.Top   + e.Value.Height / 2 - 10);
                    _window._Canvas.Children.Add(r);

                }
                SetText(r, e.Key);
                r.Visibility = Visibility.Visible;
                // r.IsHitTestVisible = false;
                _points.Add(e.Key,(e.Value, r));
                i++;
            }

            for (var j = points.rects.Count; j < childrenCount; j++)
            {
                _window._Canvas.Children[j].Visibility = Visibility.Hidden;
            }

            Console.WriteLine("CreateHint:" + w.ElapsedMilliseconds);
        }
    }
}