using System.Collections.Generic;

namespace Metatool.UIElementsDetector
{
    public interface IUIElementsDetector
    {
        List<UIElement> Detect();
    }
}
