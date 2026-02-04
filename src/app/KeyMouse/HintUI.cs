using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace KeyMouse
{
    /// <summary>
    /// Manages the hint UI overlay for displaying detected objects with keyboard shortcuts
    /// </summary>
    public class HintUI
    {
        private static HintUI _instance;
        public static HintUI Inst => _instance ?? (_instance = new HintUI());

        private MainWindow _window;
        private Dictionary<string, (Rect rect, TextBlock hint)> _hints;
        private List<TextBlock> _markedHints;

        private HintUI()
        {
            _window = MainWindow.Instance;
            _hints = new Dictionary<string, (Rect rect, TextBlock hint)>();
            _markedHints = new List<TextBlock>();
        }

        /// <summary>
        /// Creates hint UI elements for each detected object
        /// </summary>
        public void CreateHint((Rect windowRect, Dictionary<string, Rect> rects) points)
        {
            _hints.Clear();
            _markedHints.Clear();

            var windowRect = points.windowRect;
            _window.Top = windowRect.Top;
            _window.Left = windowRect.Left;
            _window.Width = windowRect.Width;
            _window.Height = windowRect.Height;

            var canvas = _window.HintCanvas;
            var childCount = canvas.Children.Count;
            var i = 0;

            foreach (var kvp in points.rects)
            {
                var key = kvp.Key;
                var rect = kvp.Value;

                TextBlock textBlock;
                if (i < childCount)
                {
                    textBlock = canvas.Children[i] as TextBlock;
                    if (textBlock != null)
                    {
                        Canvas.SetLeft(textBlock, rect.Left + rect.Width / 2 - 10);
                        Canvas.SetTop(textBlock, rect.Top + rect.Height / 2 - 10);
                    }
                }
                else
                {
                    var yellowColor = Colors.Yellow;
                    yellowColor.A = 0xaa;

                    textBlock = new TextBlock
                    {
                        IsHitTestVisible = false,
                        Foreground = Brushes.Red,
                        Background = new SolidColorBrush(yellowColor),
                        FontWeight = FontWeights.Bold,
                        FontSize = 12,
                        Padding = new Thickness(4, 2, 4, 2)
                    };

                    Canvas.SetLeft(textBlock, rect.Left + rect.Width / 2 - 10);
                    Canvas.SetTop(textBlock, rect.Top + rect.Height / 2 - 10);
                    canvas.Children.Add(textBlock);
                }

                // Set text with individual Run elements for partial highlighting
                textBlock.Inlines.Clear();
                foreach (var c in key)
                {
                    var run = new Run { Text = c.ToString() };
                    textBlock.Inlines.Add(run);
                }

                textBlock.Visibility = Visibility.Visible;
                _hints.Add(key, (rect, textBlock));
                i++;
            }

            // Hide unused text blocks
            for (var j = points.rects.Count; j < childCount; j++)
            {
                canvas.Children[j].Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Shows the hint overlay
        /// </summary>
        public void Show()
        {
            _window.HintCanvas.Visibility = Visibility.Visible;
            _window.Show();
            _window.Activate();
        }

        /// <summary>
        /// Hides the hint overlay
        /// </summary>
        public void Hide()
        {
            _window.Hide();
            _window.HintCanvas.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Marks a hint as partially matched (colors the matched part)
        /// </summary>
        public void MarkHit(string key, int matchLength)
        {
            if (!_hints.TryGetValue(key, out var hintData))
                return;

            var textBlock = hintData.hint;
            _markedHints.Add(textBlock);

            int charIndex = 0;
            foreach (Run run in textBlock.Inlines)
            {
                if (charIndex < matchLength)
                {
                    run.Foreground = Brushes.Blue;
                }
                charIndex++;
            }
        }

        /// <summary>
        /// Hides a specific hint by key
        /// </summary>
        public void HideHint(string key)
        {
            if (_hints.TryGetValue(key, out var hintData))
            {
                hintData.hint.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Shows all hints again
        /// </summary>
        public void ShowHints()
        {
            _window.HintCanvas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides all hints
        /// </summary>
        public void HideAllHints()
        {
            _window.HintCanvas.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Resets hint styling after a key press sequence
        /// </summary>
        public void ResetHintStyling()
        {
            foreach (var hint in _markedHints)
            {
                foreach (Run run in hint.Inlines)
                {
                    run.ClearValue(TextElement.ForegroundProperty);
                }
            }
            _markedHints.Clear();
        }
    }
}
