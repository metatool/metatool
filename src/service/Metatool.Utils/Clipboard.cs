using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Metatool.Service;

namespace Metatool.Utils;

public class Clipboard : IClipboard
{
    private IWindowManager _windowManager;
    IWindowManager WindowManager => _windowManager ??= Services.Get<IWindowManager>();
    private IFileExplorer _fileExplorer;
    IFileExplorer FileExplorer => _fileExplorer ??= Services.Get<IFileExplorer>();

    public void SetText(string text)
    {
        UiDispatcher.Dispatcher.BeginInvoke(() =>
            System.Windows.Clipboard.SetText(text)
            );
    }

    public async Task<string> PasteAsFile(string folder = null)
    {
        if (folder == null)
        {
            if (WindowManager.CurrentWindow.IsExplorerOrOpenSaveDialog)
            {
                folder = await FileExplorer.CurrentDirectory(WindowManager.CurrentWindow.Handle);
            }

            if (string.IsNullOrEmpty(folder))
            {
                folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var targetFolder = folder;

        var filePath = await UiDispatcher.DispatchAsync(() =>
        {
            if (System.Windows.Clipboard.ContainsImage())
            {
                var image = System.Windows.Clipboard.GetImage();
                var path = Path.Combine(targetFolder, $"{timestamp}.png");
                using var stream = new FileStream(path, FileMode.Create);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                return path;
            }

            if (System.Windows.Clipboard.ContainsText())
            {
                var text = System.Windows.Clipboard.GetText();
                var path = Path.Combine(targetFolder, $"{timestamp}.txt");
                File.WriteAllText(path, text);
                return path;
            }

            return null;
        });

        return filePath;
    }
}