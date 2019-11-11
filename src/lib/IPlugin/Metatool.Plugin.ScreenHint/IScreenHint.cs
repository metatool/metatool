using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Metatool.Plugin
{
    public interface IScreenHint
    {
   Task Show(Action<(Rect winRect, Rect clientRect)> action, bool buildHints = true);
    }
}
