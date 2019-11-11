using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Metatool.Plugin;
using Metatool.ScreenPoint;

namespace Metatool.ScreenHint
{
    public sealed class ScreenHint: IScreenHint
    {
        private readonly IKeyboard _keyboard;

        public ScreenHint(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }
        static (Rect windowRect, Dictionary<string, Rect> rects) _positions;

        public  async Task Show(Action<(Rect winRect, Rect clientRect)> action, bool buildHints = true)
        {
            buildHints = buildHints || _positions.Equals(default);
            if (buildHints)
            {
                var builder = new HintsBuilder();
                _positions = builder.BuildHintPositions();
                HintUI.Inst.CreateHint(_positions);
                HintUI.Inst.Show();
            }
            else
            {
                HintUI.Inst.Show(true);
            }


            var hits = new StringBuilder();
            while (true)
            {
                var downArg = await _keyboard.KeyDownAsync(true);

                if (downArg.KeyCode == Keys.LShiftKey)
                {
                    HintUI.Inst.HideHints();
                    var upArg = await _keyboard.KeyUpAsync();
                    HintUI.Inst.ShowHints();
                    continue;
                }

                var downKey = downArg.KeyCode.ToString();
                if (downKey.Length > 1 || !Config.Keys.Contains(downKey))
                {
                    HintUI.Inst.Hide();
                    return;
                }

                hits.Append(downKey);
                var ks = new List<string>();
                foreach (var k in _positions.rects.Keys)
                {
                    if (k.StartsWith(hits.ToString()))
                    {
                        HintUI.Inst.MarkHit(k, hits.Length);
                        ks.Add(k);
                    }
                    else
                    {
                        HintUI.Inst.HideHint(k);
                    }
                }

                if (ks.Count == 0)
                {
                    HintUI.Inst.Hide();
                    return;
                }

                var key = ks.FirstOrDefault(k => k.Length == hits.Length);
                if (!string.IsNullOrEmpty(key))
                {
                    var v = _positions.rects[key];
                    HintUI.Inst.HideHints();
                    HintUI.Inst.HighLight(v);

                    await Task.Run(()=>
                    {
                        Thread.Sleep(100);
                        action((_positions.windowRect, v));
                    });
                    return;
                }
            }
        }

     }
}