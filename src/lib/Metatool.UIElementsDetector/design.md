# Task
  Create a c# lib in this Metatool.UIElementsDetector folder, can detect the user clickable object on screenshot of the current active Windows app's window, and return the regions of the objects.
# Note
* use OmniParser but only use only the detector part and do not use Florence-2 and OCR. converting the model to ONNX for using in c# proj
* get real-time performance on a CPU
* ScreenCapture.NET for screen capture, which use directX for faster capture
* Compunet.YoloV8 for object detection on screen
* ONNX (FP16 or INT8): Reduces file size and speeds up math on CPU.
* zoom the screenshot of active app into the with of 640 but keep ratio of with/height
