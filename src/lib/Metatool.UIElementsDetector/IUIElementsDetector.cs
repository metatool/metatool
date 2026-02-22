namespace Metatool.UIElementsDetector
{
    public interface IUIElementsDetector
    {
        List<UIElement> Detect(System.IntPtr windowHandle);
    }
}
