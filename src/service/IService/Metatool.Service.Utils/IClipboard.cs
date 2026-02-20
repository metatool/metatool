using System.Threading.Tasks;

namespace Metatool.Service;

public interface IClipboard
{
    void SetText(string text);
    Task<string> PasteAsFile(string folder = null);
}