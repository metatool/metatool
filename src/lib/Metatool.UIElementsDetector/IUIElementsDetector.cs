using Metatool.Service;
using Metatool.Service.ScreenHint;

namespace Metatool.UIElementsDetector
{

    public interface IUIElementsDetector
    {
        (IUIElement screen, IUIElement winRect, List<IUIElement> elements) Detect(System.IntPtr windowHandle);
    }
}
