using System;
using System.Windows;
using System.Windows.Threading;
using Metatool.Service;

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
		if (sender is System.Windows.Controls.TextBlock tb && tb.Tag is IUIElement rect)
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

	public void ShowHighLight(IUIElement rect)
	{
		var dpiScale = System.Windows.Media.VisualTreeHelper.GetDpi(this);
		System.Windows.Controls.Canvas.SetLeft(highLight, rect.X / dpiScale.DpiScaleX);
		System.Windows.Controls.Canvas.SetTop(highLight, rect.Y / dpiScale.DpiScaleY);
		highLight.Width = rect.Width / dpiScale.DpiScaleX;
		highLight.Height = rect.Height / dpiScale.DpiScaleY;
		highLight.Visibility = Visibility.Visible;
	}

	public void HideHighLight()
	{
		highLight.Visibility = Visibility.Hidden;
	}

	public void ShowLoading(IntPtr windowHandle)
	{
		ScreenCapturer.User32.GetWindowRect(windowHandle, out var rect);
		var screen = new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

		var dpiScale = System.Windows.Media.VisualTreeHelper.GetDpi(this);
		Top = screen.Y / dpiScale.DpiScaleY;
		Left = screen.X / dpiScale.DpiScaleX;
		Width = screen.Width / dpiScale.DpiScaleX;
		Height = screen.Height / dpiScale.DpiScaleY;

		_Canvas.Visibility = Visibility.Hidden;
		_loadingText.Visibility = Visibility.Visible;
		Show();

		// Force WPF to render the loading indicator before detection blocks the UI thread
		//Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
	}

	public void HideLoading()
	{
		_loadingText.Visibility = Visibility.Hidden;
	}
}