using System;

namespace Clipboard.Shared.Exceptions
{
    public sealed class NotNullOrEmptyRequiredException : ArgumentException
    {
        public NotNullOrEmptyRequiredException(string parameterName)
            : base(parameterName, "The value must not be null or empty")
        {
        }
    }
}
