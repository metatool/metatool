using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using static Metatool.Service.Key;
using Cp = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace Metatool.Service;

public class Clipboard : IClipboard
{
	private readonly IKeyboard _keyboard;

	public Clipboard(IKeyboard keyboard)
	{
		_keyboard = keyboard;
	}

	public async Task<string> CopyTextAsync(int timeout = -1, CancellationToken token = default)
	{
		var task = TaskExt.FromEvent<object>()
			.Start(h => Cp.ContentChanged += h, h => Cp.ContentChanged -= h,
				token == default ? CancellationToken.None : token);

		_keyboard.Type(Ctrl + C);

		try
		{
			var o = await task.TimeoutAfter(timeout);
		}
		catch (TimeoutException)
		{
			Console.WriteLine("timeout of copy text");
			return "";
		}

		var dataPackageView = Cp.GetContent();
		// foreach (var availableFormat in dataPackageView.AvailableFormats)
		// {
		//     Console.WriteLine(availableFormat);
		// }
		if (dataPackageView.Contains(StandardDataFormats.Text))
		{
			var text = await dataPackageView.GetTextAsync();
			Console.WriteLine(text);
			return text;
		}

		return string.Empty;
	}
}