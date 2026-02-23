using System.Runtime.InteropServices;
using ScreenCapture.NET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Metatool.ScreenCapturer
{
    public class ScreenCapturer : IScreenCapturer
    {
        private readonly IScreenCaptureService _screenCaptureService;
        private readonly List<Display> _displays;
        private IScreenCapture? _screenCapture;
        private ICaptureZone? _captureZone;

        public ScreenCapturer()
        {
            _screenCaptureService = new DX11ScreenCaptureService();
            var graphicsCards = _screenCaptureService.GetGraphicsCards();
            if (!graphicsCards.Any())
            {
                throw new InvalidOperationException("No graphics cards found for screen capture.");
            }

            _displays = graphicsCards.SelectMany(gc => _screenCaptureService.GetDisplays(gc)).ToList();

            if (!_displays.Any())
            {
                throw new InvalidOperationException("No displays found for screen capture.");
            }
        }

        public Image<Bgra32>? CaptureScreen(IntPtr windowHandle)
        {
            // Find the display that contains the window
            var display = FindDisplayForWindow(windowHandle);

            if (_screenCapture == null || _screenCapture.Display != display)
            {
                // Don't dispose _screenCapture — the service caches instances per display
                // and owns their lifecycle. Disposing here poisons the cache.
                _screenCapture = _screenCaptureService.GetScreenCapture(display);
            }

            (_captureZone as IDisposable)?.Dispose();

            // Capture the full display; the window handle is only used to pick the correct monitor
            _captureZone = _screenCapture.RegisterCaptureZone(0, 0, _screenCapture.Display.Width, _screenCapture.Display.Height);

            if (!_screenCapture.CaptureScreen())
                return null;

            return CreateImageFromZone(_captureZone);
        }

        private static unsafe Image<Bgra32> CreateImageFromZone(ICaptureZone zone)
        {
            using (zone.Lock())
            {
                var buffer = zone.RawBuffer;
                var width = zone.Width;
                var height = zone.Height;
                var stride = zone.Stride;

                var image = new Image<Bgra32>(width, height);
                fixed (byte* pBuffer = buffer)
                {
                    var ptr = pBuffer;
                    image.ProcessPixelRows(accessor =>
                    {
                        for (var y = 0; y < height; y++)
                        {
                            var pixelRow = accessor.GetRowSpan(y);
                            var sourceRow = new Span<byte>(ptr + y * stride, width * 4);
                            MemoryMarshal.Cast<byte, Bgra32>(sourceRow).CopyTo(pixelRow);
                        }
                    });
                }
                return image;
            }
        }

        private Display FindDisplayForWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero)
            {
                var hMonitor = User32.MonitorFromWindow(windowHandle, User32.MONITOR_DEFAULTTONEAREST);
                var monitorInfo = new User32.MONITORINFOEX { cbSize = Marshal.SizeOf<User32.MONITORINFOEX>() };

                if (User32.GetMonitorInfo(hMonitor, ref monitorInfo))
                {
                    var deviceName = monitorInfo.szDevice;
                    var match = _displays.FirstOrDefault(d => d.DeviceName == deviceName);
                    if (match != default)
                        return match;
                }
            }

            return _displays.First();
        }

        public void Dispose()
        {
            (_captureZone as IDisposable)?.Dispose();
            _captureZone = null;
            _screenCapture = null;

            // The service owns the IScreenCapture instances (cached per display).
            // Disposing the service disposes all cached captures and the DX factory.
            (_screenCaptureService as IDisposable)?.Dispose();
        }
    }
}
