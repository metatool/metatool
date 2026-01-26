# Task
  Create a c# app in this KeyMouse folder, that based on WPF, and can capture the screenshot of the current active app when user press the shortcut of ctrl+alt+a, then detect the user actable object on screenshot, mark the region of that active object on screen with Key letter to hint the user to type keyboard to trigger the click action of the mouse.
# Note
* get real-time performance on a CPU
* ScreenCapture.NET for screen capture
* Compunet.YoloV8 for object detection on screen
* ONNX (FP16 or INT8): Reduces file size and speeds up math on CPU.
* zoom the screenshot of active app into the with of 640 but keep ratio of with/height
* use the available api functions of projects in the Metatool folder as much as possible

M:\Workspace\metatool\src\service\Metatool.ScreenHint\HintsBuilder.cs