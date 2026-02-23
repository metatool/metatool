using Metatool.Service;

namespace Metatool.UIElementsDetector
{

    public interface IUIElementsDetector
    {
        (IUIElement screen, IUIElement winRect, List<IUIElement> elements) Detect(System.IntPtr windowHandle);
    }
}
