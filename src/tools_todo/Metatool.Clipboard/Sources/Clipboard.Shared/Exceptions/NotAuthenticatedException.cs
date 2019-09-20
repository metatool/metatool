using System;

namespace Clipboard.Shared.Exceptions
{
    public sealed class NotAuthenticatedException : Exception
    {
        public NotAuthenticatedException()
            : base("User not authenticated.")
        {
        }
    }
}
