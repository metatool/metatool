using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Metatool.ScreenHint.HintUI;

public class HintUI : IHintUI
{
	// Hint colors
	static readonly Brush HintForeground    = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
	static readonly Brush HintBackground    = new SolidColorBrush(Color.FromArgb(0xE6, 0xCC, 0x33, 0x33));
	static readonly Brush HintMatchedColor  = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0xA0));

	MainWindow _window;
	MainWindow Window => _window ??= MainWindow.Inst;

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

		Window._Canvas.Visibility = System.Windows.Visibility.Visible;

		Window.Show();
	}

	public void Hide()
	{
		Window.Hide();
	}

	public void HideHints()
	{
		Window._Canvas.Visibility = System.Windows.Visibility.Hidden;
	}

	public bool IsHintsVisible => Window._Canvas.Visibility == System.Windows.Visibility.Visible;

	List<TextBlock> markedHints = new();

	public void MarkHitKey(string key, int len)
	{
		_points.TryGetValue(key, out var ui);
		markedHints.Add(ui.hint);
		var i = 0;
		foreach (var run in ui.hint.Inlines)
		{
			if (i == len) break;
			run.Foreground = HintMatchedColor;
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
		Window._Canvas.Visibility = System.Windows.Visibility.Visible;
	}

	public void HighLight(Rect rect)
	{
		Window.HighLight(rect);
	}
	Dictionary<string, (Rect rect, TextBlock hint)> _points;
	public void CreateHint((Rect windowRect, Dictionary<string, Rect> rects) points)
	{
		_points = new Dictionary<string, (Rect rect, TextBlock hint)>();
		var w = new Stopwatch();
		w.Start();
		var rr = points.windowRect;
		Window.Top    = rr.Top;
		Window.Left   = rr.Left;
		Window.Width  = rr.Width;
		Window.Height = rr.Height;
		var childrenCount = Window._Canvas.Children.Count;
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
				r      = Window._Canvas.Children[i] as TextBlock;
				Canvas.SetLeft(r, e.Value.Left + e.Value.Width  / 2 - 10);
				Canvas.SetTop(r, e.Value.Top   + e.Value.Height / 2 - 10);
			}
			else
			{
				r = new TextBlock()
				{
					IsHitTestVisible = false,
					Foreground = HintForeground,
					Background = HintBackground,
					FontWeight = FontWeights.Bold,
					Padding = new Thickness(2, 1, 2, 1),
				};
				Canvas.SetLeft(r, e.Value.Left + e.Value.Width  / 2 - 10);
				Canvas.SetTop(r, e.Value.Top   + e.Value.Height / 2 - 10);
				Window._Canvas.Children.Add(r);

			}
			SetText(r, e.Key);
			r.Visibility = Visibility.Visible;
			// r.IsHitTestVisible = false;
			_points.Add(e.Key,(e.Value, r));
			i++;
		}

		for (var j = points.rects.Count; j < childrenCount; j++)
		{
			Window._Canvas.Children[j].Visibility = Visibility.Hidden;
		}

		Console.WriteLine("CreateHint:" + w.ElapsedMilliseconds);
	}
}