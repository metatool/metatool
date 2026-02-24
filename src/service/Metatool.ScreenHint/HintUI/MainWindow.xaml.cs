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
		Inst = this;
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

	private void _Canvas_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{

	}
}