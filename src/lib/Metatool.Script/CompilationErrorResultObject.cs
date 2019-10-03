using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;

namespace Metatool.Script
{
    internal interface IResultObject
    {
        string? Value { get; }

        void WriteTo(StringBuilder builder);
    }

    [DataContract]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public class CompilationErrorResultObject : IResultObject
    {
        // for serialization
        // ReSharper disable once UnusedMember.Local
        private CompilationErrorResultObject()
        {
            ErrorCode = string.Empty;
            Severity  = string.Empty;
            Message   = string.Empty;
        }

        [DataMember(Name = "ec")]
        public string ErrorCode { get; private set; }
        [DataMember(Name = "sev")]
        public string Severity { get; private set; }
        [DataMember(Name = "l")]
        public int Line { get; private set; }
        [DataMember(Name = "col")]
        public int Column { get; private set; }
        [DataMember(Name = "m")]
        public string Message { get; private set; }

        public static CompilationErrorResultObject Create(string severity, string errorCode, string message, string path, int line, int column)
        {
            return new CompilationErrorResultObject
            {
                ErrorCode = errorCode,
                Severity  = severity,
                Message   = message,
                Path = path,
                // 0 to 1-based
                Line   = line   + 1,
                Column = column + 1,
            };
        }
        [DataMember(Name = "p")]
        public string Path { get; private set; }

        public override string ToString() => $"{Path}@{Line}:{Column}, {Severity} {ErrorCode}: {Message}";

        string? IResultObject.Value => ToString();

        public void WriteTo(StringBuilder builder) => builder.Append(ToString());
    }
}
