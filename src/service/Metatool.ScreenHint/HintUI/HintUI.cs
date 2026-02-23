using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Metatool.Service;

namespace Metatool.ScreenHint.HintUI;

public class HintUI : IHintUI
{
	// Hint colors
	static readonly Brush HintForeground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
	static readonly Brush HintBackground = new SolidColorBrush(Color.FromArgb(0xE6, 0xCC, 0x33, 0x33));
	static readonly Brush HintMatchedColor = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0xA0));

	MainWindow _window;
	MainWindow Window => _window ??= MainWindow.Inst;

	public void Show(bool isReshow = false)
	{
		if (isReshow)
		{
			foreach (var point in _hints)
			{
				point.Value.hint.Visibility = Visibility.Visible;
			}

			foreach (var hint in markedHints)
			{
				foreach (Run run in hint.Inlines)
				{
					run.ClearValue(TextElement.ForegroundProperty);
				}
			}
		}

		Window._Canvas.Visibility = System.Windows.Visibility.Visible;

		Window.Show();
		_window.Activate();
	}

	public void Hide()
	{
		Window.Hide();
	}

	public void HideAllHints()
	{
		Window._Canvas.Visibility = System.Windows.Visibility.Hidden;
	}

	public bool IsHintsVisible => Window._Canvas.Visibility == System.Windows.Visibility.Visible;

	List<TextBlock> markedHints = new();
	/// <summary>
	/// Marks a hint as partially matched (colors the matched part)
	/// </summary>
	public void MarkHitKey(string key, int len)
	{
		if (!_hints.TryGetValue(key, out var hintData))
		{
			return;
		}

		markedHints.Add(hintData.hint);
		var charIndex = 0;
		foreach (var run in hintData.hint.Inlines)
		{
			if (charIndex == len)
				break;

			run.Foreground = HintMatchedColor;
			charIndex++;
		}
	}
	/// <summary>
	/// Resets hint styling after a key press sequence
	/// </summary>
	public void ResetHintStyling()
	{
		foreach (var hint in markedHints)
		{
			foreach (Run run in hint.Inlines)
			{
				run.ClearValue(TextElement.ForegroundProperty);
			}
		}
		markedHints.Clear();
	}

	public void HideHint(string key)
	{
		if (_hints.TryGetValue(key, out var ui))
			ui.hint.Visibility = Visibility.Hidden;
	}

	public void ShowHints()
	{
		Window._Canvas.Visibility = System.Windows.Visibility.Visible;
	}

	public void HighLight(IUIElement rect)
	{
		Window.HighLight(rect);
	}
	Dictionary<string, (IUIElement rect, TextBlock hint)> _hints;
	public void CreateHint((IUIElement windowRect, Dictionary<string, IUIElement> rects) points)
	{
		_hints = new Dictionary<string, (IUIElement rect, TextBlock hint)>();
#if DEBUG
		var w = new Stopwatch();
		w.Start();
#endif
		var rr = points.windowRect;

		// Position window on target monitor first, then query DPI.
		// On multi-DPI setups, GetDpi must be called after the window is on
		// the correct monitor to return the right per-monitor DPI values.
		var initDpi = VisualTreeHelper.GetDpi(Window);
		Window.Top = rr.Y / initDpi.DpiScaleY;
		Window.Left = rr.X / initDpi.DpiScaleX;
		Window.Width = rr.Width / initDpi.DpiScaleX;
		Window.Height = rr.Height / initDpi.DpiScaleY;

		var dpiScale = VisualTreeHelper.GetDpi(Window);
		double dpiScaleX = dpiScale.DpiScaleX;
		double dpiScaleY = dpiScale.DpiScaleY;

		if (dpiScaleX != initDpi.DpiScaleX || dpiScaleY != initDpi.DpiScaleY)
		{
			Window.Top = rr.Y / dpiScaleY;
			Window.Left = rr.X / dpiScaleX;
			Window.Width = rr.Width / dpiScaleX;
			Window.Height = rr.Height / dpiScaleY;
		}
		var canvas = Window._Canvas;
		// Scale font size with DPI so hints stay readable on high-DPI monitors
		var fontSize = 14.0 * dpiScaleX;

		var childrenCount = canvas.Children.Count;
		var i = 0;

		static void SetKeyTextWithRuns(TextBlock ui, string key)
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

		foreach (var kvp in points.rects)
		{
			TextBlock textBlock;
			var rect = kvp.Value;
			if (i < childrenCount)
			{
				textBlock = canvas.Children[i] as TextBlock;
			}
			else
			{
				textBlock = new TextBlock()
				{
					IsHitTestVisible = false,
					Foreground = HintForeground,
					Background = HintBackground,
					FontWeight = FontWeights.Bold,
					Padding = new Thickness(2, 1, 2, 1),
				};
				canvas.Children.Add(textBlock);
			}
			textBlock.FontSize = fontSize;
			// Set text with individual Run elements for partial highlighting
			SetKeyTextWithRuns(textBlock, kvp.Key);

			// Measure actual rendered size to center the hint on the element
			textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			var centerX = (rect.X + rect.Width / 2.0) / dpiScaleX;
			var centerY = (rect.Y + rect.Height / 2.0) / dpiScaleY;
			Canvas.SetLeft(textBlock, centerX - textBlock.DesiredSize.Width / 2);
			Canvas.SetTop(textBlock, centerY - textBlock.DesiredSize.Height / 2);

			textBlock.Visibility = Visibility.Visible;
			// r.IsHitTestVisible = false;
			_hints.Add(kvp.Key, (rect, textBlock));
			i++;
		}

		// Hide unused text blocks
		for (var j = points.rects.Count; j < childrenCount; j++)
		{
			canvas.Children[j].Visibility = Visibility.Hidden;
		}

#if DEBUG
		Debug.WriteLine("CreateHint:" + w.ElapsedMilliseconds);
		Debug.WriteLine($"[CreateHint] requested={points.rects.Count}, created={i}, canvasChildren={canvas.Children.Count}, hintsDict={_hints.Count}");
		canvas.UpdateLayout();
		int visibleCount = 0;
		foreach (UIElement child in canvas.Children)
		{
			if (child.Visibility == Visibility.Visible) visibleCount++;
		}
		Debug.WriteLine($"[CreateHint] visibleChildren={visibleCount}");
#endif
	}
}