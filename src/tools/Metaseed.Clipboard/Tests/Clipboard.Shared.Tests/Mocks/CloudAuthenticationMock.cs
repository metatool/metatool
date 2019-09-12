using System;
using System.Threading.Tasks;
using Clipboard.Shared.CloudStorage;

namespace Clipboard.Shared.Tests.Mocks
{
    internal class CloudAuthenticationMock : ICloudAuthentication
    {
        public Task<AuthenticationResult> AuthenticateAsync(string authenticationUri, string redirectUri)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrEmpty(authenticationUri))
                {
                    return new AuthenticationResult(true, new Uri(redirectUri));
                }

                return new AuthenticationResult(false, new Uri(redirectUri));
            });
        }
    }
}
