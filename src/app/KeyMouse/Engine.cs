using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Metatool.UIElementsDetector;
using Metatool.ScreenCapturer;
using Metatool.Input.MouseKeyHook;
using Metatool.Service.MouseKey;

namespace KeyMouse
{
    public enum DetectMode
    {
        ActiveMonitor,
        ActiveWindow,
    }

    public class Engine : IDisposable
    {
        private readonly UIElementsDetector _detector;
        private readonly HintAction _hintAction;

        private Dictionary<string, Rect> _currentHints;
        private string _typedKeySequence = "";
        private bool _isHintMode = false;
        private Rect _overlayRect;

        public DetectMode Mode { get; set; } = DetectMode.ActiveMonitor;

        public Engine(string modelPath)
        {
            _detector = new UIElementsDetector(modelPath);
            _hintAction = new HintAction(new Metatool.WindowsInput.InputSimulator());
        }

        public void Activate()
        {
            if (_isHintMode) return;

            try
            {
                var handle = User32.GetForegroundWindow();
                if (handle == IntPtr.Zero) return;

                _detector.UpdateCaptureZone(handle);
                var elements = _detector.Detect();

                if (elements.Count == 0) return;

                // Convert physical pixels to WPF DIPs for correct overlay positioning.
                var dpiScale = VisualTreeHelper.GetDpi(MainWindow.Instance);
                double dpiScaleX = dpiScale.DpiScaleX;
                double dpiScaleY = dpiScale.DpiScaleY;

                // Get the monitor info for coordinate conversion
                var hMonitor = User32.MonitorFromWindow(handle, User32.MONITOR_DEFAULTTONEAREST);
                var monitorInfo = new User32.MONITORINFOEX { cbSize = System.Runtime.InteropServices.Marshal.SizeOf<User32.MONITORINFOEX>() };
                User32.GetMonitorInfo(hMonitor, ref monitorInfo);
                int monitorLeft = monitorInfo.rcMonitor.Left;
                int monitorTop = monitorInfo.rcMonitor.Top;
                int monitorW = monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left;
                int monitorH = monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top;

                User32.GetWindowRect(handle, out var rect);

                var rects = new List<Rect>();

                if (Mode == DetectMode.ActiveMonitor)
                {
                    // Overlay covers the full monitor; element coords are monitor-relative
                    _overlayRect = new Rect(
                        monitorLeft / dpiScaleX,
                        monitorTop / dpiScaleY,
                        monitorW / dpiScaleX,
                        monitorH / dpiScaleY);

                    foreach (var el in elements)
                    {
                        rects.Add(new Rect(
                            el.X / dpiScaleX,
                            el.Y / dpiScaleY,
                            el.Width / dpiScaleX,
                            el.Height / dpiScaleY));
                    }
                }
                else
                {
                    // Overlay covers the active window; element coords are window-relative
                    _overlayRect = new Rect(
                        rect.Left / dpiScaleX,
                        rect.Top / dpiScaleY,
                        (rect.Right - rect.Left) / dpiScaleX,
                        (rect.Bottom - rect.Top) / dpiScaleY);

                    int winX = rect.Left - monitorLeft;
                    int winY = rect.Top - monitorTop;
                    int winW = rect.Right - rect.Left;
                    int winH = rect.Bottom - rect.Top;

                    foreach (var el in elements)
                    {
                        // Convert from display-relative to window-relative
                        var relX = el.X - winX;
                        var relY = el.Y - winY;

                        // Skip elements outside the active window
                        if (relX + el.Width < 0 || relY + el.Height < 0 || relX >= winW || relY >= winH)
                            continue;

                        rects.Add(new Rect(
                            relX / dpiScaleX,
                            relY / dpiScaleY,
                            el.Width / dpiScaleX,
                            el.Height / dpiScaleY));
                    }
                }

                if (rects.Count == 0) return;

                _currentHints = KeyGenerator.GetKeyPointPairs(rects, Config.Keys);

                var hintData = new Dictionary<string, Rect>();
                foreach (var kvp in _currentHints) hintData.Add(kvp.Key, kvp.Value);

                HintUI.Inst.CreateHint((_overlayRect, hintData));
#if DEBUG
                Debug.WriteLine($"[Hints] mode={Mode}, {hintData.Count} hints, overlay=({_overlayRect.X:F0},{_overlayRect.Y:F0},{_overlayRect.Width:F0},{_overlayRect.Height:F0}), dpi={dpiScaleX:F2}x{dpiScaleY:F2}");
                foreach (var kvp in hintData)
                    Debug.WriteLine($"  key={kvp.Key} region=({kvp.Value.X:F0},{kvp.Value.Y:F0},{kvp.Value.Width:F0},{kvp.Value.Height:F0})");
#endif
                HintUI.Inst.Show();

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

            if (!Config.Keys.Contains(keyChar))
            {
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
            HintUI.Inst.CreateHint((_overlayRect, hintData));
            HintUI.Inst.Show();

            _isHintMode = true;
            _typedKeySequence = "";
        }

        private void ExitHintMode()
        {
            _isHintMode = false;
            _typedKeySequence = "";
            HintUI.Inst.ResetHintStyling();
            HintUI.Inst.Hide();
        }

        public void Dispose()
        {
            (_detector as IDisposable)?.Dispose();
        }
    }
}
