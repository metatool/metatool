using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Metatool.ScreenCapturer
{
    public interface IScreenCapturer : IDisposable
    {
        Image<Bgra32>? CaptureScreen(IntPtr windowHandle);
    }
}
