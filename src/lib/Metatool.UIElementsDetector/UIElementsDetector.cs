using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ScreenCapture.NET;
using OpenCvSharp;
using YoloSharp.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Metatool.UIElementsDetector
{
    public class UIElementsDetector : IDisposable
    {
        private YoloModel _model;
        private string _modelPath;
        private IScreenCaptureService _screenCaptureService;
        private IScreenCapture _screenCapture;
        private ICaptureZone _captureZone;

        public UIElementsDetector(string modelPath, IntPtr windowHandle = default)
        {
            _modelPath = modelPath;
            if (!File.Exists(_modelPath))
            {
                throw new FileNotFoundException($"Model file not found at {_modelPath}");
            }

            // Initialize YoloSharp
            _model = new YoloModel(_modelPath);

            // Initialize ScreenCapture
            _screenCaptureService = new DX11ScreenCaptureService();
            var graphicsCards = _screenCaptureService.GetGraphicsCards();
            if (!graphicsCards.Any())
            {
                throw new InvalidOperationException("No graphics cards found for screen capture.");
            }

            var displays = _screenCaptureService.GetDisplays(graphicsCards.First());

            if (!displays.Any())
            {
                throw new InvalidOperationException("No displays found on the primary graphics card for screen capture.");
            }

            _screenCapture = _screenCaptureService.GetScreenCapture(displays.First());

            if (_screenCapture == null)
            {
                throw new InvalidOperationException("Failed to get screen capture service. It returned null.");
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

            if (windowHandle != IntPtr.Zero)
            {
                if (User32.GetWindowRect(windowHandle, out User32.RECT rect))
                {
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    if (width <= 0 || height <= 0 || rect.Left < 0 || rect.Top < 0)
                    {
                         _captureZone = _screenCapture.RegisterCaptureZone(0, 0, _screenCapture.Display.Width, _screenCapture.Display.Height);
                    }
                    else
                    {
                        _captureZone = _screenCapture.RegisterCaptureZone(rect.Left, rect.Top, width, height);
                    }
                }
                else
                {
                     _captureZone = _screenCapture.RegisterCaptureZone(0, 0, _screenCapture.Display.Width, _screenCapture.Display.Height);
                }
            }
            else
            {
                 _captureZone = _screenCapture.RegisterCaptureZone(0, 0, _screenCapture.Display.Width, _screenCapture.Display.Height);
            }
        }

        public unsafe List<UIElement> Detect()
        {
            if (_screenCapture == null || _captureZone == null)
            {
                return new List<UIElement>();
            }

            _screenCapture.CaptureScreen();

            using (_captureZone.Lock())
            {
                var buffer = _captureZone.RawBuffer;
                int width = _captureZone.Width;
                int height = _captureZone.Height;
                int stride = _captureZone.Stride; // Stride is in bytes

                // Create ImageSharp Image from raw buffer (BGRA)
                using (var image = new Image<Bgra32>(width, height))
                {
                    fixed (byte* pBuffer = buffer)
                    {
                        byte* ptr = pBuffer;
                        image.ProcessPixelRows(accessor => {
                            for (int y = 0; y < height; y++)
                            {
                                var pixelRow = accessor.GetRowSpan(y);
                                var sourceRow = new Span<byte>(ptr + y * stride, width * 4);
                                MemoryMarshal.Cast<byte, Bgra32>(sourceRow).CopyTo(pixelRow);
                            }
                        });
                    }

                    // Resize to width 640 while maintaining aspect ratio, per requirements.
                    int targetWidth = 640;
                    int targetHeight = (int)((double)height / width * targetWidth);

                    image.Mutate(x => x.Resize(targetWidth, targetHeight));

                    var result = _model.RunInference(image);

                    var elements = new List<UIElement>();

                    // Iterate through YoloSharp predictions
                    foreach (var prediction in result.Predictions)
                    {
                        var bbox = prediction.Rectangle;

                        // Map back to original coordinates
                        double scaleX = (double)width / targetWidth;
                        double scaleY = (double)height / targetHeight;

                        elements.Add(new UIElement
                        {
                            X = (int)(bbox.X * scaleX),
                            Y = (int)(bbox.Y * scaleY),
                            Width = (int)(bbox.Width * scaleX),
                            Height = (int)(bbox.Height * scaleY),
                            Confidence = prediction.Score,
                            Label = prediction.Class.Name
                        });
                    }

                    return elements;
                }
            }
        }

        public unsafe Mat CaptureActiveWindowImage()
        {
            if (_screenCapture == null || _captureZone == null)
            {
                return null;
            }

            _screenCapture.CaptureScreen();

            using (_captureZone.Lock())
            {
                var buffer = _captureZone.RawBuffer;
                int width = _captureZone.Width;
                int height = _captureZone.Height;
                int stride = _captureZone.Stride;

                fixed (byte* p = buffer)
                {
                    return Mat.FromPixelData(height, width, MatType.CV_8UC4, (IntPtr)p, stride).Clone();
                }
            }
        }

        public void Dispose()
        {
             (_model as IDisposable)?.Dispose();

             if (_captureZone is IDisposable disposableZone)
             {
                 disposableZone.Dispose();
             }
        }
    }

    public class UIElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Confidence { get; set; }
        public string Label { get; set; }
    }
}
