using System.Diagnostics;
using Compunet.YoloSharp;
using Compunet.YoloSharp.Plotting;
using Metatool.Service;
using SixLabors.ImageSharp;

namespace Metatool.UIElementsDetector
{
    public class UIElementsDetector : IUIElementsDetector, IDisposable
    {
        private readonly YoloPredictor _model;
        private readonly ScreenCapturer.IScreenCapturer _screenCapturer;

        public UIElementsDetector() : this(new ScreenCapturer.ScreenCapturer()) { }

        public UIElementsDetector(ScreenCapturer.IScreenCapturer screenCapturer)
        {
            _screenCapturer = screenCapturer;

            var modelPath = Path.Combine(
                Path.GetDirectoryName(GetType().Assembly.Location)!,
                "icon_detect.onnx");

            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"UI elements detection model file not found at {modelPath}");
            }

            // Initialize YoloSharp
            _model = new YoloPredictor(modelPath);
            // Minimum confidence score for a detection to be kept (default: 0.25).
            // Lower values return more detections but may include false positives.
            _model.Configuration.Confidence = 0.05f;
            // IoU (Intersection over Union) threshold for NMS (default: 0.45).
            // Lower values suppress more overlapping boxes; higher values keep more nearby detections.
            _model.Configuration.IoU = 0.45f;
        }

        public static IUIElement GetScreenRect(IntPtr windowHandle)
        {
            // Get the monitor info for coordinate conversion
            var hMonitor = ScreenCapturer.User32.MonitorFromWindow(windowHandle, ScreenCapturer.User32.MONITOR_DEFAULTTONEAREST);
            var monitorInfo = new ScreenCapturer.User32.MONITORINFOEX { cbSize = System.Runtime.InteropServices.Marshal.SizeOf<ScreenCapturer.User32.MONITORINFOEX>() };
            ScreenCapturer.User32.GetMonitorInfo(hMonitor, ref monitorInfo);
            int monitorLeft = monitorInfo.rcMonitor.Left;
            int monitorTop = monitorInfo.rcMonitor.Top;
            int monitorW = monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left;
            int monitorH = monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top;
            return new UIElement { X = monitorLeft, Y = monitorTop, Width = monitorW, Height = monitorH };
        }

        /// <summary>
        /// Gets the absolute bounding rectangle of the specified window, position relative to the screen's top-left corner.
        /// the screen position is relative to the main screen's top-left corner
        /// </summary>
        /// <param name="windowHandle"></param>
        public static (IUIElement screen, IUIElement window) GetWindowRect(IntPtr windowHandle)
        {
            var screen = GetScreenRect(windowHandle);
            ScreenCapturer.User32.GetWindowRect(windowHandle, out var rect);
            var winRect = new UIElement { X = rect.Left - screen.X, Y = rect.Top - screen.Y, Width = rect.Right - rect.Left, Height = rect.Bottom - rect.Top };
            return (screen, winRect);
        }
        /// <summary>
        /// Detects all visible, enabled UI automation elements within the screen that has the specified window
        /// and returns their bounding rectangles relative to the screen's top-left corner.
        ///
        /// </summary>
        public (IUIElement screen, IUIElement winRect, List<IUIElement> elements) Detect(IntPtr windowHandle)
        {
            var winRect = GetWindowRect(windowHandle);
            using var image = _screenCapturer.CaptureScreen(windowHandle);
            if (image == null)
                return (winRect.screen, winRect.window, new List<IUIElement>());

#if DEBUG_
            var tempDir = @"c:\temp\1";
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
            image.SaveAsPng(Path.Combine(tempDir, $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.png"));
#endif

            var result = _model.Detect(image);
#if DEBUG_
            Debug.WriteLine($"[Detect] {result.Count} detections (image: {image.Width}x{image.Height}, speed: {result.Speed})");
            foreach (var p in result)
                Debug.WriteLine($"  [{p.Name.Name}] conf={p.Confidence:F3} bounds=({p.Bounds.X},{p.Bounds.Y},{p.Bounds.Width},{p.Bounds.Height})");

            using var annotatedImage = result.PlotImage(image);
            annotatedImage.SaveAsPng(Path.Combine(tempDir, $"detected_{DateTime.Now:yyyyMMdd_HHmmss}.png"));
#endif
            var elements = new List<IUIElement>();

            foreach (var prediction in result)
            {
                var bbox = prediction.Bounds;
                elements.Add(new UIElement
                {
                    X = bbox.X,
                    Y = bbox.Y,
                    Width = bbox.Width,
                    Height = bbox.Height,
                    Confidence = prediction.Confidence,
                    Label = prediction.Name.Name
                });
            }

            return (winRect.screen, winRect.window, elements);
        }

        public static List<IUIElement> ToWindowRelative(IUIElement winRect, List<IUIElement> elements, bool filterOutWinElements = true)
        {
            var result = new List<IUIElement>();

            // var WinXAbs = winRect.X + screen.X;
            // var eXAbs = el.X + screen.X;
            // var eXToWin = eXAbs - WinXAbs;(so screen.X is not needed)
            foreach (var el in elements)
            {
                var x = el.X - winRect.X;
                var y = el.Y - winRect.Y;
                if (filterOutWinElements && (x < 0 || y < 0 || x > winRect.Width || y > winRect.Height))
                {
                    // Skip elements that are outside the window bounds (likely belong to other windows)
                    continue;
                }

                result.Add(new UIElement
                {
                    X = x,
                    Y = y,
                    Width = el.Width,
                    Height = el.Height,
                    Confidence = (el as UIElement)?.Confidence ?? 0f,
                    Label = (el as UIElement)?.Label ?? ""
                });
            }

            return result;
        }
        public void Dispose()
        {
            (_model as IDisposable)?.Dispose();
            _screenCapturer?.Dispose();
        }
    }

    public class UIElement : IUIElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Confidence { get; set; }
        public string Label { get; set; }
    }
}
