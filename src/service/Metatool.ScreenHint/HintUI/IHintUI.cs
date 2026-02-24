using System;
using System.Collections.Generic;
using System.Windows;
using Metatool.Service;

namespace Metatool.ScreenHint.HintUI;

public interface IHintUI
{
	void ShowCreatingHint(IntPtr windowHandle);
	void ShowHints();
	void Show(bool isReshow = false);
	void Hide();
	void HideHint(string key);
	void HideAllHints();
	bool IsHintsVisible { get; }
	void MarkHitKey(string key, int len);
	void HighLight(IUIElement rect);
	void CreateHint((IUIElement windowRect, Dictionary<string, IUIElement> rects) points);
}
