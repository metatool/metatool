using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Metatool.Service;
using Metatool.ScreenPoint;
using Metatool.Service.MouseKey;
using Metatool.ScreenHint.HintUI;

namespace Metatool.ScreenHint;

public sealed class ScreenHint(IKeyboard keyboard, IUiDispatcher dispatcher, IHintsBuilder hintsBuilder, IUIElementsDetector detector, IWindowManager windowManager, IHintUI hintUI) : IScreenHint
{
	(Rect windowRect, Dictionary<string, Rect> rects) _positions;

	public async Task Show(Action<(Rect winRect, Rect clientRect)> action, bool buildHints = true)
	{
		if (!dispatcher.CheckAccess())
		{
			await dispatcher.DispatchAsync(() => Show(action, buildHints));
			return;
		}
		buildHints = buildHints || _positions.Equals(default);
		if (buildHints)
		{
			var winHandle = windowManager.CurrentWindow.Handle;
			var (winRect, elementRects) = detector.Detect(winHandle);//run in UI thread to avoid COMException in UIAutomation
			_positions = (winRect, hintsBuilder.BuildHintPositions(elementRects));
			hintUI.CreateHint(_positions);
			hintUI.Show();
		}
		else
		{
			hintUI.Show(true);
		}


		var hits = new StringBuilder();
		while (true)
		{
			var downArg = await keyboard.KeyDownAsync(true);

			if (downArg.KeyCode == KeyCodes.LShiftKey)
			{
				hintUI.HideHints();
				var upArg = await keyboard.KeyUpAsync();
				hintUI.ShowHints();
				continue;
			}

			var downKey = downArg.KeyCode.ToString();
			if (downKey.Length > 1 || !Config.Keys.Contains(downKey))
			{
				hintUI.Hide();
				return;
			}

			hits.Append(downKey);
			var ks = new List<string>();
			foreach (var k in _positions.rects.Keys)
			{
				if (k.StartsWith(hits.ToString()))
				{
					hintUI.MarkHitKey(k, hits.Length);
					ks.Add(k);
				}
				else
				{
					hintUI.HideHint(k);
				}
			}

			if (ks.Count == 0)
			{
				hintUI.Hide();
				return;
			}

			var key = ks.FirstOrDefault(k => k.Length == hits.Length);
			if (!string.IsNullOrEmpty(key))
			{
				var v = _positions.rects[key];
				hintUI.HideHints();
				hintUI.HighLight(v);

				await Task.Run(() =>
				{
					Thread.Sleep(100);
					action((_positions.windowRect, v));
				});
				return;
			}
		}
	}

}