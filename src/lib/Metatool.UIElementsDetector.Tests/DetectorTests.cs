using NUnit.Framework;
using Metatool.UIElementsDetector;
using Metatool.ScreenCapturer;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using OpenCvSharp; // Added for Mat type

namespace Metatool.UIElementsDetector.Tests
{
    public class DetectorTests
    {
        private string _modelPath;

        [SetUp]
        public void Setup()
        {
            _modelPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "icon_detect.onnx");
            Assert.That(File.Exists(_modelPath), $"Model file not found at {_modelPath}");
        }

        [Test]
        public void Detect_Performance_ShouldBeLessThan800ms()
        {
            UIElementsDetector detector = null;
            try 
            {
                detector = new UIElementsDetector(_modelPath);
            }
            catch (Exception ex)
            {
                Assert.Ignore($"Skipping test: Failed to initialize detector (likely no display or GPU support): {ex.Message}");
                return;
            }

            // Warm-up run (optional, but standard for performance tests to exclude JIT and cold caches)
            try
            {
                detector.Detect();
            }
            catch (Exception ex)
            {
                 Assert.Ignore($"Skipping test: Detect failed during warm-up (likely no display): {ex.Message}");
                 return;
            }

            // Measure performance
            Stopwatch sw = Stopwatch.StartNew();
            var results = detector.Detect(); 
            sw.Stop();

            TestContext.WriteLine($"Detection took: {sw.ElapsedMilliseconds} ms");
            TestContext.WriteLine($"Detected {results.Count} elements.");

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(800), "Detection took longer than 800ms");
            Assert.That(results, Is.Not.Null);
        }

        [Test]
        public void DetectAndOutput_ShouldOutputImageAndJson()
        {
            TestContext.WriteLine("Please ensure the window you wish to capture is active and foreground BEFORE this test runs.");
            System.Threading.Thread.Sleep(2000); // Give user a moment to switch if they read the output

            // 1. Get the foreground window handle
            IntPtr foregroundWindowHandle = User32.GetForegroundWindow();
            TestContext.WriteLine($"Foreground Window Handle: {foregroundWindowHandle}");
            Assert.That(foregroundWindowHandle != IntPtr.Zero, "No foreground window found. Please ensure a window is active.");

            if (User32.GetWindowRect(foregroundWindowHandle, out User32.RECT rect))
            {
                TestContext.WriteLine($"Window Rect: Left={rect.Left}, Top={rect.Top}, Right={rect.Right}, Bottom={rect.Bottom}");
                TestContext.WriteLine($"Window Dimensions: Width={rect.Right - rect.Left}, Height={rect.Bottom - rect.Top}");
            }
            else
            {
                TestContext.WriteLine("Could not get window rectangle.");
            }

            UIElementsDetector detector = null;
            try
            {
                // Initialize detector with the foreground window handle
                // This might throw if GetWindowRect returns invalid coordinates
                detector = new UIElementsDetector(_modelPath, foregroundWindowHandle);
            }
            catch (InvalidOperationException ex)
            {
                TestContext.WriteLine($"Detector initialization failed with invalid window dimensions: {ex.Message}");
                Assert.Ignore($"Skipping test: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                Assert.Ignore($"Skipping test: Failed to initialize detector (likely no display or cannot capture window): {ex.Message}");
                return;
            }

            // Perform detection
            using var capturedMat = detector.CaptureActiveWindowImage();
            var results = detector.Detect();

            // 3. Save the captured image
            var imageOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "captured_window.png");
            try
            {
                if (capturedMat != null)
                {
                    capturedMat.SaveImage(imageOutputPath);
                    TestContext.WriteLine($"Captured image saved to: {imageOutputPath}");
                    Assert.That(File.Exists(imageOutputPath), "Captured image file was not created.");
                }
                else
                {
                    Assert.Fail("Captured image (Mat) was null.");
                }
            }
            finally
            {
                // capturedMat is a using var, so it will be disposed
            }

            // 4. Serialize detected regions to JSON
            var jsonOutputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "detected_regions.json");
            var jsonString = System.Text.Json.JsonSerializer.Serialize(results, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonOutputPath, jsonString);
            TestContext.WriteLine($"Detected regions JSON saved to: {jsonOutputPath}");
            Assert.That(File.Exists(jsonOutputPath), "Detected regions JSON file was not created.");

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Not.Empty, "No UI elements detected. Ensure a window with UI elements is active.");
        }
    }
}
