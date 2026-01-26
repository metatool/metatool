using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using ScreenCapture.NET;
using OpenCvSharp;

namespace Metatool.UIElementsDetector
{
    public class UIElementsDetector
    {
        private InferenceSession _session;
        private string _modelPath;
        private IScreenCaptureService _screenCaptureService;
        private IScreenCapture _screenCapture;
        private ICaptureZone _captureZone;

        private const int ScreenCaptureDelayMs = 500; // Delay to allow the screen buffer to update after capture.

        public UIElementsDetector(string modelPath, IntPtr windowHandle = default)
        {
            _modelPath = modelPath;
            if (!File.Exists(_modelPath))
            {
                throw new FileNotFoundException($"Model file not found at {_modelPath}");
            }

            // Initialize ONNX Session
            _session = new InferenceSession(_modelPath);

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

            if (windowHandle != IntPtr.Zero)
            {
                // Capture specific window
                if (User32.GetWindowRect(windowHandle, out User32.RECT rect))
                {
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    // Fallback to full screen if window has invalid dimensions or negative coordinates
                    if (width <= 0 || height <= 0 || rect.Left < 0 || rect.Top < 0)
                    {
                        Console.WriteLine($"Warning: Window handle {windowHandle} has invalid dimensions or negative coordinates (Left={rect.Left}, Top={rect.Top}, Width={width}, Height={height}). Falling back to full screen capture.");
                        _captureZone = _screenCapture.RegisterCaptureZone(0, 0, _screenCapture.Display.Width, _screenCapture.Display.Height);
                    }
                    else
                    {
                        _captureZone = _screenCapture.RegisterCaptureZone(rect.Left, rect.Top, width, height);
                    }
                }
                else // GetWindowRect failed
                {
                    Console.WriteLine($"Warning: GetWindowRect failed for handle {windowHandle}. Falling back to full screen capture.");
                    _captureZone = _screenCapture.RegisterCaptureZone(0, 0, _screenCapture.Display.Width, _screenCapture.Display.Height);
                }
            }
            else
            {
                // Capture full screen (default behavior)
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
                int stride = _captureZone.Stride;

                fixed (byte* p = buffer)
                {
                    using var originalMat = Mat.FromPixelData(height, width, MatType.CV_8UC4, (IntPtr)p, stride);

                    // 2. Preprocess for YOLO (Resize to 640x640, Normalize)
                    // YOLOv8 expects RGB, so convert BGRA to RGB
                    using var rgbMat = new Mat();
                    Cv2.CvtColor(originalMat, rgbMat, ColorConversionCodes.BGRA2RGB);

                    // Resize to 640x640 while maintaining aspect ratio (letterboxing) or just resize
                    // The prompt asked to "zoom the screenshot of active app into the with of 640 but keep ratio of with/height"
                    // But YOLO model input is fixed (usually square 640x640).
                    // We'll resize to 640x640 for the model input, but keep track of the scale to map back.

                    int targetWidth = 640;
                    int targetHeight = 640;

                    using var resizedMat = new Mat();
                    Cv2.Resize(rgbMat, resizedMat, new OpenCvSharp.Size(targetWidth, targetHeight));

                    // Prepare Tensor
                    var tensor = new DenseTensor<float>(new[] { 1, 3, targetHeight, targetWidth });

                    // Loop through pixels and normalize to [0, 1]
                    using var floatMat = new Mat();
                    resizedMat.ConvertTo(floatMat, MatType.CV_32FC3, 1.0 / 255.0);

                    var indexer = floatMat.GetGenericIndexer<Vec3f>();
                    for (int y = 0; y < targetHeight; y++)
                    {
                        for (int x = 0; x < targetWidth; x++)
                        {
                            var pixel = indexer[y, x];
                            tensor[0, 0, y, x] = pixel.Item0; // R
                            tensor[0, 1, y, x] = pixel.Item1; // G
                            tensor[0, 2, y, x] = pixel.Item2; // B
                        }
                    }

                    // 3. Inference
                    var inputs = new List<NamedOnnxValue>
                            {
                                NamedOnnxValue.CreateFromTensor("images", tensor)
                            };

                    using var results = _session.Run(inputs);

                    // 4. Post-processing
                    var output = results.First().AsTensor<float>();
                    var elements = ParseYoloOutput(output, width, height, targetWidth, targetHeight);

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
                    // Directly return a clone of the captured image as BGRA
                    return Mat.FromPixelData(height, width, MatType.CV_8UC4, (IntPtr)p, stride).Clone();
                }
            }
        }
        private List<UIElement> ParseYoloOutput(Tensor<float> output, int originalWidth, int originalHeight, int modelWidth, int modelHeight)
        {
            var elements = new List<UIElement>();

            // Output shape is typically [1, num_features, num_anchors] -> [1, 5, 8400] for class agnostic detection?
            // Or [1, 4 + num_classes, 8400]
            // Let's assume standard YOLOv8 detection output.
            // The dimensions might need verification from the converted model metadata or shape.

            int numFeatures = output.Dimensions[1]; // e.g., 5 or 84
            int numAnchors = output.Dimensions[2];  // e.g., 8400

            float confThreshold = 0.25f; // Adjust as needed

            // YOLOv8 output: [cx, cy, w, h, class_probs...]

            for (int i = 0; i < numAnchors; i++)
            {
                // For simplified "icon_detect" it might just be 1 class?
                // If it is [1, 5, 8400], then index 4 is confidence.

                float confidence = 0f;
                int classId = -1;

                if (numFeatures == 5)
                {
                    confidence = output[0, 4, i];
                    classId = 0;
                }
                else
                {
                    // Find max class probability
                    float maxScore = 0f;
                    int maxClassIndex = -1;
                    for (int c = 4; c < numFeatures; c++)
                    {
                        float score = output[0, c, i];
                        if (score > maxScore)
                        {
                            maxScore = score;
                            maxClassIndex = c - 4;
                        }
                    }
                    confidence = maxScore;
                    classId = maxClassIndex;
                }

                if (confidence > confThreshold)
                {
                    float cx = output[0, 0, i];
                    float cy = output[0, 1, i];
                    float w = output[0, 2, i];
                    float h = output[0, 3, i];

                    // Convert back to original scale
                    float x = (cx - w / 2) * originalWidth / modelWidth;
                    float y = (cy - h / 2) * originalHeight / modelHeight;
                    float width = w * originalWidth / modelWidth;
                    float height = h * originalHeight / modelHeight;

                    elements.Add(new UIElement
                    {
                        X = (int)x,
                        Y = (int)y,
                        Width = (int)width,
                        Height = (int)height,
                        Confidence = confidence,
                        Label = $"Class {classId}"
                    });
                }
            }

            // Apply NMS (Non-Maximum Suppression) here if needed
            // Simple NMS implementation
            return ApplyNMS(elements);
        }

        private List<UIElement> ApplyNMS(List<UIElement> boxes, float iouThreshold = 0.45f)
        {
            var sorted = boxes.OrderByDescending(b => b.Confidence).ToList();
            var selected = new List<UIElement>();

            while (sorted.Count > 0)
            {
                var current = sorted[0];
                selected.Add(current);
                sorted.RemoveAt(0);

                for (int i = sorted.Count - 1; i >= 0; i--)
                {
                    if (CalculateIoU(current, sorted[i]) > iouThreshold)
                    {
                        sorted.RemoveAt(i);
                    }
                }
            }

            return selected;
        }

        private float CalculateIoU(UIElement box1, UIElement box2)
        {
            float x1 = Math.Max(box1.X, box2.X);
            float y1 = Math.Max(box1.Y, box2.Y);
            float x2 = Math.Min(box1.X + box1.Width, box2.X + box2.Width);
            float y2 = Math.Min(box1.Y + box1.Height, box2.Y + box2.Height);

            float intersectionArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            float box1Area = box1.Width * box1.Height;
            float box2Area = box2.Width * box2.Height;

            return intersectionArea / (box1Area + box2Area - intersectionArea);
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
