namespace Metatool.UIElementsDetector
{
    public interface IUIElementsDetector
    {
        List<UIElement> Detect();
        void UpdateCaptureZone(System.IntPtr windowHandle);
    }
}
