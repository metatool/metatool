# Metatool.UIElementsDetector

A C# library for detecting clickable UI elements on the screen using the OmniParser YOLOv8 model.

## Features

*   **Real-time Screen Capture**: Uses `ScreenCapture.NET` with DirectX 11 for high-performance screen capturing.
*   **Object Detection**: Uses a converted YOLOv8 ONNX model (from `microsoft/OmniParser-v2.0`) running on `Microsoft.ML.OnnxRuntime`.
*   **Efficient Processing**: Leverages `OpenCvSharp` and unsafe memory access for fast image pre-processing.
*   **Output**: Returns a list of `UIElement` objects containing bounding boxes and confidence scores.

## Usage

```csharp
var detector = new UIElementsDetector("path/to/icon_detect.onnx");
var elements = detector.Detect();

foreach (var element in elements)
{
    Console.WriteLine($"Found element at ({element.X}, {element.Y}) with size {element.Width}x{element.Height}");
}
```

## Dependencies

*   ScreenCapture.NET
*   ScreenCapture.NET.DX11
*   Microsoft.ML.OnnxRuntime
*   OpenCvSharp4
*   OpenCvSharp4.runtime.win

## Notes

*   The ONNX model `icon_detect.onnx` must be provided to the constructor.
*   The detector resizes the screen capture to 640x640 for the model input but maps the coordinates back to the original screen resolution.
