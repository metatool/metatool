using System.Diagnostics;
using Compunet.YoloSharp;
using SixLabors.ImageSharp;

namespace Metatool.UIElementsDetector
{
    public class UIElementsDetector : IDisposable
    {
        private readonly YoloPredictor _model;
        private readonly ScreenCapturer.ScreenCapturer _screenCapturer;

        public UIElementsDetector()
        {
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
            _model.Configuration.IoU = 0.4f;

            // Initialize ScreenCapturer
            _screenCapturer = new ScreenCapturer.ScreenCapturer();
        }

        public List<UIElement> Detect(IntPtr windowHandle)
        {
            using var image = _screenCapturer.CaptureScreen(windowHandle);
            if (image == null)
                return new List<UIElement>();

#if DEBUG
            var tempDir = @"c:\temp\1";
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
            image.SaveAsPng(Path.Combine(tempDir, $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.png"));
#endif

            var result = _model.Detect(image);
#if DEBUG
            Debug.WriteLine($"[Detect] {result.Count} detections (image: {image.Width}x{image.Height}, speed: {result.Speed})");
            foreach (var p in result)
                Debug.WriteLine($"  [{p.Name.Name}] conf={p.Confidence:F3} bounds=({p.Bounds.X},{p.Bounds.Y},{p.Bounds.Width},{p.Bounds.Height})");
#endif
            var elements = new List<UIElement>();

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

            return elements;
        }

        public void Dispose()
        {
            (_model as IDisposable)?.Dispose();
            _screenCapturer?.Dispose();
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
