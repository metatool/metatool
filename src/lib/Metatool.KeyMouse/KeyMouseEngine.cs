using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Metatool.UIElementsDetector;
using Metatool.ScreenCapturer;
using Metatool.Service;
using Metatool.Service.MouseKey;
using UIElement = Metatool.ScreenPoint.UIElement;

namespace KeyMouse
{
    public enum DetectMode
    {
        ActiveMonitor,
        ActiveWindow,
    }

    public class KeyMouseEngine(Config config, KeyMouseMainWindow overlayWindow) : IDisposable
    {
        private readonly UIElementsDetector _detector = new();
        private readonly HintAction _hintAction = new(new Metatool.WindowsInput.InputSimulator());
        private readonly HintUI _hintUI = new(overlayWindow);
        private readonly Visual _dpiReference = overlayWindow;

        private Dictionary<string, Rect> _currentHints;
        private string _typedKeySequence = "";
        private bool _isHintMode = false;
        private Rect _overlayRect;

        public DetectMode Mode { get; set; } = DetectMode.ActiveWindow;

        public void Activate()
        {
            if (_isHintMode) return;

            try
            {
                var handle = User32.GetForegroundWindow();
                if (handle == IntPtr.Zero) return;

                var (screen, winRect, elementPositions) = _detector.Detect(handle);

                if (elementPositions.Count == 0) return;

                IUIElement outerRect; // abs pos to main screen left,top
                List<IUIElement> elementRects; // relative to rect
                if (Mode == DetectMode.ActiveWindow)
                {
                    outerRect = new UIElement() { X = winRect.X + screen.X, Y = winRect.Y + screen.Y, Width = winRect.Width, Height = winRect.Height };
                    // position relative to WindowRect
                    elementRects = UIElementsDetector.ToWindowRelative(winRect, elementPositions);
                }
                else
                {
                    outerRect = screen;
                    elementRects = elementPositions;
                }

                if (elementRects.Count == 0) return;
                var rects = elementRects.Select(r => new Rect(r.X, r.Y, r.Width, r.Height)).ToList();

                _currentHints = KeyGenerator.GetKeyPointPairs(rects, config.Keys);

                var hintData = new Dictionary<string, Rect>();
                foreach (var kvp in _currentHints) hintData.Add(kvp.Key, kvp.Value);

                _hintUI.CreateHint((_overlayRect, hintData));
#if DEBUG
                Debug.WriteLine($"[Hints] mode={Mode}, {hintData.Count} hints, overlay=({_overlayRect.X:F0},{_overlayRect.Y:F0},{_overlayRect.Width:F0},{_overlayRect.Height:F0})");
                foreach (var kvp in hintData)
                    Debug.WriteLine($"  key={kvp.Key} region=({kvp.Value.X:F0},{kvp.Value.Y:F0},{kvp.Value.Width:F0},{kvp.Value.Height:F0})");
#endif
                _hintUI.Show();

                _isHintMode = true;
                _typedKeySequence = "";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                ExitHintMode();
            }
        }

        public void HandleKeyDown(object sender, IKeyEventArgs e)
        {
            //  Debug.WriteLine($"key: {e.Key}");
            if (!_isHintMode) return;
            if (e.KeyCode == KeyCodes.Escape)
            {
                ExitHintMode();
                return;
            }
            // Ignore keys with modifiers held (e.g. the "A" from Ctrl+Alt+A activation)
            if (e.Control || e.Alt) return;

            // Prevent other apps from processing the key
            e.Handled = true;

            // Check if key is valid char
            string keyChar = e.KeyCode.ToString().ToUpper();

            // Allow Backspace
            if (e.KeyCode == KeyCodes.Back)
            {
                if (_typedKeySequence.Length > 0)
                {
                    _typedKeySequence = _typedKeySequence.Substring(0, _typedKeySequence.Length - 1);
                    UpdateHints();
                }
                return;
            }

            if (!config.Keys.Contains(keyChar))
            {
                return;
            }

            _typedKeySequence += keyChar;
            UpdateHints();
        }

        private void UpdateHints()
        {
            _hintUI.ResetHintStyling();

            var matches = new List<string>();
            foreach (var key in _currentHints.Keys)
            {
                if (key.StartsWith(_typedKeySequence))
                {
                    _hintUI.MarkHit(key, _typedKeySequence.Length);
                    matches.Add(key);
                }
                else
                {
                    _hintUI.HideHint(key);
                }
            }

            if (matches.Count == 0)
            {
                ExitHintMode();
                return;
            }

            if (matches.Count == 1 && matches[0] == _typedKeySequence)
            {
                var target = _currentHints[matches[0]];
                ExitHintMode();
                _hintAction.Execute(_overlayRect, target);
            }
        }

        public void Reshow()
        {
            if (_isHintMode || _currentHints == null || _currentHints.Count == 0) return;

            var hintData = new Dictionary<string, Rect>(_currentHints);
            _hintUI.CreateHint((_overlayRect, hintData));
            _hintUI.Show();

            _isHintMode = true;
            _typedKeySequence = "";
        }

        private void ExitHintMode()
        {
            _isHintMode = false;
            _typedKeySequence = "";
            _hintUI.ResetHintStyling();
            _hintUI.Hide();
        }

        public void Dispose()
        {
            (_detector as IDisposable)?.Dispose();
        }
    }
}
