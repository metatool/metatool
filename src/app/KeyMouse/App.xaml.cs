using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Metatool.UIElementsDetector;
using Metatool.WindowsInput;
using Metatool.MouseKeyHook;
using Metatool.Input.MouseKeyHook;
using System.Windows.Forms; // For Keys
using System.Windows.Media;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using System.IO;

namespace KeyMouse
{
    public partial class App : Application
    {
        private IKeyboardMouseEvents _globalHook;
        private UIElementsDetector _detector;
        private InputSimulator _inputSimulator;

        // Key mapping state
        private Dictionary<string, Rect> _currentHints;
        private string _typedKeySequence = "";
        private bool _isHintMode = false;
        private Rect _activeWindowRect;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon_detect.onnx");
             if (!File.Exists(modelPath))
             {
                 System.Windows.MessageBox.Show($"Model not found: {modelPath}");
                 Shutdown();
                 return;
             }

             try {
                _detector = new UIElementsDetector(modelPath);
             } catch (Exception ex) {
                 System.Windows.MessageBox.Show($"Failed to init detector: {ex.Message}");
                 Shutdown();
                 return;
             }

            _inputSimulator = new InputSimulator();

            _globalHook = Hook.GlobalEvents();

            _globalHook.OnCombination(new Dictionary<Combination, Action>
            {
                { Combination.TriggeredBy(Keys.A).With(Keys.Control, Keys.Alt), OnActivate }
            });

            _globalHook.KeyDown += GlobalHook_KeyDown;
        }

        private void OnActivate()
        {
             if (_isHintMode) return;

             try {
                IntPtr handle = User32.GetForegroundWindow();
                if (handle == IntPtr.Zero) return;

                _detector.UpdateCaptureZone(handle);
                var elements = _detector.Detect();

                if (elements.Count == 0) return;

                // Get Window Rect to offset hints if needed?
                // UIElementsDetector returns coordinates relative to the capture zone (which matches the window).
                // HintUI expects coordinates relative to the Window passed in.

                User32.RECT rect;
                User32.GetWindowRect(handle, out rect);
                _activeWindowRect = new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

                var rects = new List<Rect>();
                foreach (var el in elements)
                {
                    // Filter low confidence? Done in detector.
                    rects.Add(new Rect(el.X, el.Y, el.Width, el.Height));
                }

                _currentHints = KeyGenerator.GetKeyPointPairs(rects, Config.Keys);

                var hintData = new Dictionary<string, Rect>();
                foreach(var kvp in _currentHints) hintData.Add(kvp.Key, kvp.Value);

                HintUI.Inst.CreateHint((_activeWindowRect, hintData));
                HintUI.Inst.Show();

                _isHintMode = true;
                _typedKeySequence = "";

             } catch (Exception ex) {
                 System.Windows.MessageBox.Show(ex.Message);
                 ExitHintMode();
             }
        }

        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isHintMode) return;

            // Prevent other apps from processing the key
            e.Handled = true;

            if (e.KeyCode == Keys.Escape)
            {
                ExitHintMode();
                return;
            }

            // Check if key is valid char
            string keyChar = e.KeyCode.ToString().ToUpper();
            // Handle number keys or others if needed. Config.Keys usually has letters.

            // Allow Backspace
            if (e.KeyCode == Keys.Back)
            {
                if (_typedKeySequence.Length > 0)
                {
                    _typedKeySequence = _typedKeySequence.Substring(0, _typedKeySequence.Length - 1);
                    UpdateHints();
                }
                return;
            }

            if (!Config.Keys.Contains(keyChar))
            {
                // Ignore or Beep?
                return;
            }

            _typedKeySequence += keyChar;
            UpdateHints();
        }

        private void UpdateHints()
        {
            HintUI.Inst.ResetHintStyling();

            var matches = new List<string>();
            foreach (var key in _currentHints.Keys)
            {
                if (key.StartsWith(_typedKeySequence))
                {
                    HintUI.Inst.MarkHit(key, _typedKeySequence.Length);
                    matches.Add(key);
                }
                else
                {
                    HintUI.Inst.HideHint(key);
                }
            }

            if (matches.Count == 0)
            {
                // No match, exit hint mode
                ExitHintMode();
                return;
            }

            if (matches.Count == 1 && matches[0] == _typedKeySequence)
            {
                // Exact match executed
                var target = _currentHints[matches[0]];
                TriggerClick(target);
                ExitHintMode();
            }
        }

        private void TriggerClick(Rect relativeRect)
        {
            // Calculate absolute screen coordinates
            // Rect is relative to _activeWindowRect
            double x = _activeWindowRect.Left + relativeRect.X + relativeRect.Width / 2;
            double y = _activeWindowRect.Top + relativeRect.Y + relativeRect.Height / 2;

            // Move mouse to the calculated position and click
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)x, (int)y);

            // Small delay to ensure mouse position is set before clicking
            System.Threading.Thread.Sleep(50);

            _inputSimulator.Mouse.LeftButtonClick();
        }

        private void ExitHintMode()
        {
            _isHintMode = false;
            _typedKeySequence = "";
            HintUI.Inst.ResetHintStyling();
            HintUI.Inst.Hide();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _globalHook?.Dispose();
            base.OnExit(e);
        }
    }
}