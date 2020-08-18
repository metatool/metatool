using System.Text;

namespace Metatool.Script {
    internal interface IResultObject
    {
        string Value { get; }
        void WriteTo(StringBuilder builder);
    }
}