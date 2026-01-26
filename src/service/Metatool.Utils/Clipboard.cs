using Metatool.Service;

namespace Metatool.Utils;

public class Clipboard : IClipboard
{
    public void SetText(string text)
    {
        UiDispatcher.Dispatcher.BeginInvoke(() =>
            System.Windows.Clipboard.SetText(text)
            );
    }
}