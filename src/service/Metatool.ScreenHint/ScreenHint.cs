using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Service;
using Metatool.ScreenPoint;
using Metatool.Service.MouseKey;
using Metatool.ScreenHint.HintUI;
using Metatool.UIElementsDetector;
using Microsoft.Extensions.Logging;
using Metatool.Service.ScreenHint;

namespace Metatool.ScreenHint;

public sealed class ScreenHint : IScreenHint, IDisposable
{
	private IUIElementsDetector detector = new UIElementsDetector.UIElementsDetector();
    private IUIElementsDetector detectorWpf;
    private IUIElementsDetector DetectorWpf => detectorWpf??= new WpfUIElementsDetector();

	/// <summary>
	/// used for show without rebuild hints, e.g. when user hold shift to see hints, then release shift to hide hints, then press another key to show hints again, in this case we don't need to rebuild hints, just show it again.
	/// </summary>
	(IUIElement windowRect, Dictionary<string, IUIElement> rects) _positions;

	private readonly IKeyboard _keyboard;
	private readonly IUiDispatcher _dispatcher;
	private readonly IHintsBuilder _hintsBuilder;
	private readonly IWindowManager _windowManager;
	private readonly IHintUI _hintUi;
	private readonly ILogger _logger;

	public ScreenHint(IKeyboard keyboard, IUiDispatcher dispatcher, IHintsBuilder hintsBuilder, IWindowManager windowManager, IHintUI hintUi, ILogger<ScreenHint> logger)
	{
		_keyboard = keyboard;
		_dispatcher = dispatcher;
		_hintsBuilder = hintsBuilder;
		_windowManager = windowManager;
		_hintUi = hintUi;
		_logger = logger;
	}

	public async Task Show(Action<(IUIElement winRect, IUIElement clientRect)> action, bool buildHints = true, bool activeWindowOnly = false,  bool useWpfDetector = false)
	{
		if (!_dispatcher.CheckAccess())
		{
			await _dispatcher.DispatchAsync(() => Show(action, buildHints, activeWindowOnly, useWpfDetector));
			return;
		}
		buildHints = buildHints || _positions.Equals(default);
		if (buildHints)
		{
			var winHandle = _windowManager.CurrentWindow.Handle;
			_hintUi.ShowCreatingHintMessage(winHandle, activeWindowOnly);

			var detector = useWpfDetector ? DetectorWpf : this.detector;
			var (screen, winRect, elementPositions) = detector.Detect(winHandle);//run in UI thread to avoid COMException in UIAutomation
			if (elementPositions.Count == 0)
			{
				_hintUi.Hide();
				_logger.LogWarning("No UI elements detected in window {0}", winHandle);
				return;
			}
			IUIElement outerRect; // abs pos to main screen left,top
			List<IUIElement> elementRects; // relative to rect
			if (activeWindowOnly)
			{
				outerRect = winRect;
				// position relative to WindowRect
				elementRects = UIElementsDetector.UIElementsDetector.ToWindowRelative(screen, winRect, elementPositions);
			}
			else
			{
				outerRect = screen;
				elementRects = elementPositions;
			}

			if (elementRects.Count == 0) {
				_logger.LogWarning("No UI elements detected in window {0} after filtering by activeWindowOnly={1}", winHandle, activeWindowOnly);
				return;
			}

			elementRects = DeduplicateByCenter(elementRects);

			var keys = _hintsBuilder.GenerateKeys(elementRects);
			_positions = (outerRect, keys);
			_hintUi.CreateHint(_positions);
			_hintUi.Show();
		}
		else
		{
			_hintUi.Show(true);
		}


		var hits = new StringBuilder();
		while (true)
		{
			var downArg = await _keyboard.KeyDownAsync(true);

			if (downArg.KeyCode == KeyCodes.LShiftKey)
			{
				_hintUi.HideAllHints();
				var upArg = await _keyboard.KeyUpAsync();
				// reverse the z order so hints hidden by others become visible
				_hintUi.ReverseHintsZOrder();
				_hintUi.ShowHints();
				continue;
			}
			if (downArg.KeyCode == KeyCodes.Back)
			{
				if (hits.Length > 0)
					hits.Remove(hits.Length - 1, 1);
				// re-show all hints with original styling
				_hintUi.Show(true);
				// re-narrow based on remaining prefix
				if (hits.Length > 0)
				{
					var hitsStr = hits.ToString();
					foreach (var k in _positions.rects.Keys)
					{
						if (k.StartsWith(hitsStr))
							_hintUi.MarkHitKey(k, hits.Length);
						else
							_hintUi.HideHint(k);
					}
				}
				continue;
			}

			var downKey = downArg.Key.KeyName;
			if (downKey.Length > 1)
			{
				_hintUi.Hide();
				return;
			}

			hits.Append(downKey);
			var ks = new List<string>();
			foreach (var k in _positions.rects.Keys)
			{
				if (k.StartsWith(hits.ToString()))
				{
					_hintUi.MarkHitKey(k, hits.Length);
					ks.Add(k);
				}
				else
				{
					_hintUi.HideHint(k);
				}
			}

			if (ks.Count == 0)
			{
				_hintUi.Hide();
				return;
			}

			var key = ks.FirstOrDefault(k => k.Length == hits.Length);
			if (!string.IsNullOrEmpty(key))
			{
				var v = _positions.rects[key];
				_hintUi.HideAllHints();
				_hintUi.HighLight(v);

				await Task.Run(() =>
				{
					Thread.Sleep(100);
					action((_positions.windowRect, v));
				});
				return;
			}
		}

	}
	private static List<IUIElement> DeduplicateByCenter_NoEfficient(List<IUIElement> elements, int threshold = 15)
	{
		var result = new List<IUIElement>();
		foreach (var elem in elements)
		{
			var cx = elem.X + elem.Width / 2;
			var cy = elem.Y + elem.Height / 2;
			var tooClose = false;
			foreach (var existing in result)
			{
				var ex = existing.X + existing.Width / 2;
				var ey = existing.Y + existing.Height / 2;
				if (Math.Abs(cx - ex) < threshold && Math.Abs(cy - ey) < threshold)
				{
					tooClose = true;
					break;
				}
			}
			if (!tooClose)
				result.Add(elem);
		}
		return result;
	}
	private static List<IUIElement> DeduplicateByCenter(List<IUIElement> elements, int threshold = 12)
	{
		var result = new List<IUIElement>();
		var occupied = new HashSet<(int, int)>();

		foreach (var elem in elements)
		{
			var cx = elem.X + elem.Width / 2;
			var cy = elem.Y + elem.Height / 2;
			var gx = cx / threshold;
			var gy = cy / threshold;

			var tooClose = false;
			for (var dx = -1; dx <= 1 && !tooClose; dx++)
			for (var dy = -1; dy <= 1 && !tooClose; dy++)
				tooClose = occupied.Contains((gx + dx, gy + dy));

			if (!tooClose)
			{
				occupied.Add((gx, gy));
				result.Add(elem);
			}
		}
		return result;
	}

	public void Dispose()
	{
		(detector as IDisposable)?.Dispose();
	}

}