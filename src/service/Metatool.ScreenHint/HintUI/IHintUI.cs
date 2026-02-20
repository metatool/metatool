using System.Collections.Generic;
using System.Windows;

namespace Metatool.ScreenHint.HintUI;

public interface IHintUI
{
	void ShowHints();
	void Show(bool isReshow = false);
	void Hide();
	void HideHint(string key);
	void HideHints();
	bool IsHintsVisible { get; }
	void MarkHitKey(string key, int len);
	void HighLight(Rect rect);
	void CreateHint((Rect windowRect, Dictionary<string, Rect> rects) points);
}
