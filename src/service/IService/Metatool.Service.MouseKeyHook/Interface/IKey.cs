namespace Metatool.Service.MouseKey;

public interface IKey
{
    /// <summary>
    /// user friendly name of the key, instead of Keycode from toString
    /// </summary>
    string KeyName { get; }
    KeyEventType Handled { get; }
}