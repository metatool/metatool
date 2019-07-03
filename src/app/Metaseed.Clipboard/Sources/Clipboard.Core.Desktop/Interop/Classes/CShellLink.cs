using System.Runtime.InteropServices;
using Clipboard.Core.Desktop.ComponentModel;

namespace Clipboard.Core.Desktop.Interop.Classes
{
    /// <summary>
    /// The shell link.
    /// </summary>
    [ComImport]
    [Guid(Consts.CShellLink)]
    [ClassInterface(ClassInterfaceType.None)]
    internal class CShellLink
    {
    }
}
