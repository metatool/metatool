using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ScreenCapture.NET;
using OpenCvSharp;

namespace Metatool.ScreenCapturer
{
    public class ScreenCapturer : IDisposable
    {
        private readonly IScreenCaptureService _screenCaptureService;
        private readonly List<Display> _displays;
        private IScreenCapture _screenCapture;
        private ICaptureZone _captureZone;

        public ScreenCapturer(IntPtr windowHandle = default)
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

            UpdateCaptureZone(windowHandle);
        }

        public void UpdateCaptureZone(IntPtr windowHandle)
        {
            if (_captureZone != null)
            {
                if (_captureZone is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _captureZone = null;
            }

            // Find the display that contains the window
            var display = FindDisplayForWindow(windowHandle);

            if (_screenCapture == null || _screenCapture.Display != display)
            {
                (_screenCapture as IDisposable)?.Dispose();
                _screenCapture = _screenCaptureService.GetScreenCapture(display);
            }

            // Capture the full display; the window handle is only used to pick the correct monitor
            _captureZone = _screenCapture.RegisterCaptureZone(0, 0, _screenCapture.Display.Width, _screenCapture.Display.Height);
        }

        public bool CaptureScreen()
        {
            if (_screenCapture == null || _captureZone == null)
                return false;

            return _screenCapture.CaptureScreen();
        }

        public IDisposable LockZone()
        {
            return _captureZone.Lock();
        }

        public ReadOnlySpan<byte> RawBuffer => _captureZone.RawBuffer;
        public int Width => _captureZone.Width;
        public int Height => _captureZone.Height;
        public int Stride => _captureZone.Stride;

        public bool IsReady => _screenCapture != null && _captureZone != null;

        public unsafe Mat CaptureAsMatrix()
        {
            if (!IsReady)
                return null;

            _screenCapture.CaptureScreen();

            using (_captureZone.Lock())
            {
                var buffer = _captureZone.RawBuffer;
                var width = _captureZone.Width;
                var height = _captureZone.Height;
                var stride = _captureZone.Stride;

                fixed (byte* p = buffer)
                {
                    return Mat.FromPixelData(height, width, MatType.CV_8UC4, (IntPtr)p, stride).Clone();
                }
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
            if (_captureZone is IDisposable disposableZone)
            {
                disposableZone.Dispose();
            }
            (_screenCapture as IDisposable)?.Dispose();
        }
    }
}
