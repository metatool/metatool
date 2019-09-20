using System;

namespace Clipboard.Shared.Exceptions
{
    public sealed class IsFalseRequiredException : Exception
    {
        public IsFalseRequiredException()
            : base("The value must be false")
        {
        }
    }
}
