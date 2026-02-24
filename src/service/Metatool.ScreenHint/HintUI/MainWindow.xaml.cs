using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Metatool.Service;
using Metatool.UIElementsDetector;

namespace Metatool.ScreenHint.HintUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	internal static MainWindow Inst = new();

	private MainWindow()
	{
		InitializeComponent();
		Hide();
		PreviewMouseMove += (s, e) => _suppressMouseEnter = false;
		Inst = this;
	}

	bool _suppressMouseEnter;

	private void _Canvas_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is true)
			_suppressMouseEnter = true;
	}

	internal void HintTextBlock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
	{
		if (_suppressMouseEnter) return;
		if (sender is TextBlock tb && tb.Tag is IUIElement rect)
		{
			ShowHighLight(rect);
			// Make hints transparent so the highlight rectangle is clearly visible
			// note: show not set to 0, otherwise it the same a hidden and mouse leave event will be triggered directly.
			_Canvas.Opacity = 0.01;
		}
	}

	internal void HintTextBlock_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
	{
		_Canvas.Opacity = 1.0;
		HideHighLight();
	}

	public void HighLight(IUIElement rect)
	{
		ShowHighLight(rect);

		DispatcherTimer timer = null;
		timer = new DispatcherTimer(TimeSpan.FromMilliseconds(800), DispatcherPriority.Input,
			(o, e) =>
			{
				highLight.Visibility = Visibility.Hidden;
				Hide();
				timer?.Stop();
			}, Dispatcher.CurrentDispatcher);
		timer.Start();
	}

	public (double dipScaleX, double dipScaleY) MoveTo(IUIElement rr)
	{
		// Position window on target monitor first, then query DPI.
		// On multi-DPI setups, GetDpi must be called after the window is on
		// the correct monitor to return the right per-monitor DPI values.
		var initDpi = VisualTreeHelper.GetDpi(this);
		Top = rr.Y / initDpi.DpiScaleY;
		Left = rr.X / initDpi.DpiScaleX;
		Width = rr.Width / initDpi.DpiScaleX;
		Height = rr.Height / initDpi.DpiScaleY;

		var dpiScale = VisualTreeHelper.GetDpi(this);
		double dpiScaleX = dpiScale.DpiScaleX;
		double dpiScaleY = dpiScale.DpiScaleY;

		if (dpiScaleX != initDpi.DpiScaleX || dpiScaleY != initDpi.DpiScaleY)
		{
			Top = rr.Y / dpiScaleY;
			Left = rr.X / dpiScaleX;
			Width = rr.Width / dpiScaleX;
			Height = rr.Height / dpiScaleY;
		}
		return (dpiScaleX, dpiScaleY);
	}

	public void ShowHighLight(IUIElement rect)
	{
		var dpiScale = VisualTreeHelper.GetDpi(this);
		Canvas.SetLeft(highLight, rect.X / dpiScale.DpiScaleX);
		Canvas.SetTop(highLight, rect.Y / dpiScale.DpiScaleY);
		highLight.Width = rect.Width / dpiScale.DpiScaleX;
		highLight.Height = rect.Height / dpiScale.DpiScaleY;
		highLight.Visibility = Visibility.Visible;
	}

	public void HideAllHints()
	{
		Left = -9999;Width = 0; Height = 0;Top = -9999;
		_Canvas.Visibility = Visibility.Hidden;
		Show();
		ForceRefresh();
	}

	public void HideHighLight()
	{
		highLight.Visibility = Visibility.Hidden;
	}

	public void ShowLoading(IntPtr windowHandle)
	{
		// Clear old hints so the stale bitmap has nothing to flash
		HideAllHints();
		_loadingText.Visibility = Visibility.Visible;
		var screen = UIElementsDetector.UIElementsDetector.GetScreenRect(windowHandle);

		var dpiScale = VisualTreeHelper.GetDpi(this);
		Top = screen.Y / dpiScale.DpiScaleY;
		Left = screen.X / dpiScale.DpiScaleX;
		Width = screen.Width / dpiScale.DpiScaleX;
		Height = screen.Height / dpiScale.DpiScaleY;

		Show();
		ForceRefresh();
	}

	/// <summary>
	/// Pumps the dispatcher message loop until all pending operations (including
	/// WPF rendering) have completed. Unlike Dispatcher.Invoke at Render priority,
	/// PushFrame creates a nested message loop that reliably processes all queued work.
	/// </summary>
	public void ForceRefresh()
	{
		var frame = new DispatcherFrame();
		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
			new Action(() => frame.Continue = false));
		Dispatcher.PushFrame(frame);
	}

	public void HideLoading()
	{
		_loadingText.Visibility = Visibility.Hidden;
	}
}