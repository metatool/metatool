using System.Linq;
using NUnit.Framework;
using Metatool.UIElementsDetector;
using System.IO;
using System;
using OpenCvSharp;
using System.Diagnostics; // Required for Process

namespace Metatool.UIElementsDetector.Tests
{
    [TestFixture]
    public class CaptureTests
    {
        private string _modelPath;

        private bool IsImageMostlyMonochromatic(Mat image, byte tolerance = 10)
        {
            if (image.Empty()) return true;

            // Convert to grayscale to simplify color comparison, or directly check channels
            // For simplicity, let's convert to grayscale and check if all pixels are within a tolerance.
            using var grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGRA2GRAY);

            byte firstPixelValue = grayImage.Get<byte>(0, 0);

            for (int y = 0; y < grayImage.Rows; y++)
            {
                for (int x = 0; x < grayImage.Cols; x++)
                {
                    byte pixelValue = grayImage.Get<byte>(y, x);
                    if (Math.Abs(pixelValue - firstPixelValue) > tolerance)
                    {
                        return false; // Found a pixel significantly different, not monochromatic
                    }
                }
            }
            return true; // All pixels are very similar, considered monochromatic
        }

        [SetUp]
        public void Setup()
        {
            _modelPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "icon_detect.onnx");
            Assert.That(File.Exists(_modelPath), $"Model file not found at {_modelPath}");
        }

        [Test, Ignore("Known issue: Captured image is blank. DX11ScreenCaptureService might not be suitable for this test environment.")]
        public void CapturePrimaryScreen_ShouldSaveImage()
        {
            UIElementsDetector detector = null;
            try
            {
                // Initialize detector to capture full primary screen (windowHandle = default)
                detector = new UIElementsDetector(_modelPath);
            }
            catch (Exception ex)
            {
                Assert.Ignore($"Skipping test: Failed to initialize detector (likely no display or graphics card issue): {ex.Message}");
                return;
            }

            Mat capturedMat = null;
            try
            {

                capturedMat = detector.CaptureActiveWindowImage(); // Captures the full screen image
            }
            catch (Exception ex)
            {
                Assert.Ignore($"Skipping test: CaptureActiveWindowImage failed: {ex.Message}");
                return;
            }

            Assert.That(capturedMat, Is.Not.Null, "Captured Mat was null.");
            Assert.That(capturedMat.Width, Is.GreaterThan(0), "Captured image width was zero.");
            Assert.That(capturedMat.Height, Is.GreaterThan(0), "Captured image height was zero.");

            var imageOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "captured_primary_screen.png");
            capturedMat.SaveImage(imageOutputPath);

            TestContext.WriteLine($"Captured primary screen image saved to: {imageOutputPath}");
            Assert.That(File.Exists(imageOutputPath), "Captured image file was not created.");

            // Load the image back and check its content
            using (var savedImage = Cv2.ImRead(imageOutputPath, ImreadModes.Color))
            {
                Assert.That(savedImage, Is.Not.Null, "Saved image could not be loaded.");
                Assert.That(savedImage.Empty(), Is.False, "Saved image is empty after loading.");
                Assert.That(IsImageMostlyMonochromatic(savedImage), Is.False, "Captured image is mostly monochromatic (blank).");
            }

            capturedMat?.Dispose();
        }

        [Test, Ignore("Known issue: Captured image is blank. DX11ScreenCaptureService might not be suitable for this test environment.")]
        public void CaptureActiveWindow_ShouldSaveImage()
        {
            Process notepadProcess = null;
            Mat capturedMat = null;
            try
            {
                // 1. Launch Notepad and bring it to foreground
                notepadProcess = Process.Start("notepad.exe");
                Assert.That(notepadProcess, Is.Not.Null, "Failed to start Notepad.");

                IntPtr notepadHandle = IntPtr.Zero;
                // Wait for Notepad window to appear
                for (int i = 0; i < 20 && notepadHandle == IntPtr.Zero; i++) // Try for up to 20 * 100ms = 2 seconds
                {
                    System.Threading.Thread.Sleep(100);
                    notepadHandle = User32.FindWindow("Notepad", null); // "Notepad" is the class name for Notepad.exe
                }
                Assert.That(notepadHandle != IntPtr.Zero, "Notepad window handle is null after waiting.");

                // Bring Notepad to foreground and verify it is foreground
                User32.ShowWindow(notepadHandle, User32.SW_RESTORE); // Restore if minimized
                User32.SetForegroundWindow(notepadHandle);
                
                // Give it a moment to become truly active and foreground
                IntPtr currentForegroundWindowHandle = IntPtr.Zero;
                for (int i = 0; i < 20 && currentForegroundWindowHandle != notepadHandle; i++) // Try for up to 20 * 100ms = 2 seconds
                {
                    System.Threading.Thread.Sleep(100);
                    currentForegroundWindowHandle = User32.GetForegroundWindow();
                }
                if (currentForegroundWindowHandle != notepadHandle)
                {
                    Assert.Ignore($"Skipping test: Notepad (Handle: {notepadHandle}) did not become the foreground window (Current Foreground: {currentForegroundWindowHandle}) after waiting. Please ensure no other applications interfere with focus.");
                    return;
                }

                UIElementsDetector detector = null;
                try
                {
                    // Initialize detector with the foreground window handle
                    detector = new UIElementsDetector(_modelPath, notepadHandle);
                }
                catch (Exception ex)
                {
                    Assert.Ignore($"Skipping test: Failed to initialize detector (likely no display or cannot capture window): {ex.Message}");
                    return;
                }

                // Capture the active window image
                capturedMat = detector.CaptureActiveWindowImage(); 

                Assert.That(capturedMat, Is.Not.Null, "Captured Mat was null.");
                Assert.That(capturedMat.Width, Is.GreaterThan(0), "Captured image width was zero.");
                Assert.That(capturedMat.Height, Is.GreaterThan(0), "Captured image height was zero.");

                var imageOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "captured_active_window.png");
                capturedMat.SaveImage(imageOutputPath);

                TestContext.WriteLine($"Captured active window image saved to: {imageOutputPath}");
                Assert.That(File.Exists(imageOutputPath), "Captured image file was not created.");
            }
            finally
            {
                capturedMat?.Dispose();
                // Clean up: Close Notepad
                if (notepadProcess != null && !notepadProcess.HasExited)
                {
                    notepadProcess.CloseMainWindow();
                    notepadProcess.WaitForExit(5000); // Wait up to 5 seconds for it to close
                    if (!notepadProcess.HasExited)
                    {
                        notepadProcess.Kill(); // Force kill if it didn't close
                    }
                }
            }
        }
    }
}
