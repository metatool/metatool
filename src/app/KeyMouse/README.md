# KeyMouse Application

A WPF-based screen analysis and interaction tool that detects UI elements on the active window and provides keyboard shortcuts to interact with them.

## Features

- **Real-time Object Detection**: Uses YOLOv8 with ONNX runtime for CPU-efficient object detection
- **Screen Capture**: Captures screenshots of the active application window using ScreenCapture.NET
- **Keyboard Navigation**: Press Ctrl+Alt+A to activate, then type keys to click on detected UI elements
- **Multi-threaded Hotkey Handling**: Global hotkey listener for seamless integration

## Architecture

### Core Components

1. **App.xaml.cs** - Application entry point and main logic
   - Manages global hotkey listener (Ctrl+Alt+A)
   - Coordinates screen capture and object detection
   - Handles keyboard input for hint selection
   - Triggers mouse clicks on selected elements

2. **MainWindow.xaml** - Transparent overlay window
   - Full-screen transparent window for displaying hints
   - Canvas-based layout for hint positioning

3. **HintUI.cs** - UI overlay manager
   - Creates and manages hint elements (TextBlocks with key labels)
   - Handles hint styling and highlighting
   - Shows/hides hint overlays

4. **KeyGenerator.cs** - Key assignment algorithm
   - Generates unique keyboard shortcuts for each detected element
   - Uses base-N numbering with configurable key set
   - Supports nested shortcuts for many elements

5. **Config.cs** - Configuration constants
   - Defines available keyboard keys (default: ASDFQWERZXCVTGBHJKLYUIOPNM)
   - Customizable for different keyboard layouts

6. **Program.cs** - Entry point
   - Contains Main() method with exception handling

## Dependencies

### NuGet Packages
- **Microsoft.ML.OnnxRuntime** (v1.23.2) - ONNX model inference
- **OpenCvSharp4** (v4.11.0+) - Image processing
- **ScreenCapture.NET** (v3.0.0) - Screen capture
- **ScreenCapture.NET.DX11** (v3.0.0) - DirectX 11 screen capture backend
- **YoloV8** (v5.3.0) - YOLO model support
- **Metatool.UIElementsDetector** - Custom UI element detection
- **Metatool.MouseKeyHook** - Global hotkey handling
- **Metatool.WindowsInput** - Input simulation

### Project References
- `Metatool.UIElementsDetector` - Object detection engine
- `Metatool.MouseKeyHook` - Global hotkey support
- `Metatool.WindowsInput` - Mouse and keyboard input

## How It Works

### Activation Flow
1. User presses **Ctrl+Alt+A** (global hotkey)
2. Application captures the active window
3. YOLOv8 object detector analyzes the screenshot
4. Detected objects are displayed with keyboard hints (yellow boxes with red text)

### Interaction Flow
1. User types letters from the hint labels
2. Matching hints are highlighted in blue
3. Exact match triggers a mouse click at the object's center
4. Hint mode exits automatically

### Key Features
- **Partial Matching**: Type partial sequences to filter down to specific elements
- **Backspace Support**: Remove characters from the sequence to reconsider options
- **Escape to Cancel**: Press Escape to exit hint mode without selecting
- **Fast Performance**: ONNX INT8 model for quick inference on CPU

## Configuration

### Keyboard Keys
Edit `Config.cs` to customize available keys:
```csharp
public static string Keys = "ASDFQWERZXCVTGBHJKLYUIOPNM";
```

The keys should be ordered by frequency or position for optimal typing speed.

## Model File

The application requires `icon_detect.onnx` model file:
- **Location**: Same directory as the executable
- **Purpose**: Detects UI elements (buttons, text fields, icons, etc.)
- **Format**: ONNX (Open Neural Network Exchange)
- **Optimization**: Recommend INT8 quantization for CPU performance

## Error Handling

- Missing model file: Shows error dialog and exits gracefully
- Detector initialization failure: Shows error details and exits
- Window no longer active: Silently ignores detection request
- No detected elements: Exits hint mode without displaying overlay

## Performance Considerations

1. **ONNX Runtime**: Uses optimized CPU inference (no GPU required)
2. **ScreenCapture.NET**: Hardware-accelerated DirectX 11 capture
3. **Image Processing**: Resizes screenshots to 640px width to maintain 4:3 aspect ratio
4. **Threading**: Global hotkey listener runs on separate thread

## Future Improvements

- [ ] Support for multiple monitor setups
- [ ] Configurable detection confidence threshold
- [ ] History of recently clicked items
- [ ] Custom hotkey configuration
- [ ] Performance profiling and optimization
- [ ] Support for other detection models (DETR, YOLOx)

## Development Notes

### File Structure
```
KeyMouse/
├── App.xaml                    # WPF Application definition
├── App.xaml.cs                 # Application logic and hotkey handling
├── MainWindow.xaml             # Overlay window UI
├── MainWindow.xaml.cs          # Window code-behind
├── Config.cs                   # Configuration constants
├── HintUI.cs                   # Hint overlay management
├── KeyGenerator.cs             # Key assignment algorithm
├── Program.cs                  # Entry point
├── KeyMouse.csproj             # Project configuration
└── design.md                   # Original design document
```

### Testing Hints
1. Open any Windows application (browser, text editor, etc.)
2. Press Ctrl+Alt+A
3. Yellow boxes with red letters should appear on clickable elements
4. Type the displayed letters to click elements

## Troubleshooting

**Problem**: Hints not appearing
- Solution: Ensure model file (icon_detect.onnx) exists in executable directory
- Solution: Check that the active window is a standard Windows window

**Problem**: Clicks not working accurately
- Solution: Ensure window hasn't moved since hint activation
- Solution: Check mouse cursor position is set correctly

**Problem**: Slow performance
- Solution: Reduce detection confidence threshold in UIElementsDetector
- Solution: Use INT8 quantized model instead of FP32

## References

- [YOLOv8 Documentation](https://docs.ultralytics.com/models/yolov8/)
- [ONNX Runtime](https://onnxruntime.ai/)
- [ScreenCapture.NET](https://github.com/CapsAdmin/ScreenCapture.NET)
