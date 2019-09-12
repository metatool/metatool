using System.Collections.Generic;
using Clipboard.Properties;
using Clipboard.Shared.CloudStorage;
using Clipboard.Shared.Logs;

namespace Clipboard.ComponentModel.CloudStorage
{
    /// <summary>
    /// Provides a set of functions designed to provide tokens for a <see cref="DropBoxProvider"/>.
    /// </summary>
    internal sealed class DropBoxTokenProvider : ICloudTokenProvider
    {
        /// <inheritdoc/>
        public string GetToken(string tokenName)
        {
            switch (tokenName)
            {
                case "AppKey":
                    return Settings.Default.DropBoxAppKey;

                case "RedirectUri":
                    return Settings.Default.DropBoxRedirectUri;

                case "AccessToken":
                    return Settings.Default.DropBoxAccessToken;

                default:
                    Logger.Instance.Error(new KeyNotFoundException($"{tokenName} not found."));
                    return string.Empty;
            }
        }

        /// <inheritdoc/>
        public void SetToken(string tokenName, string value)
        {
            switch (tokenName)
            {
                case "AccessToken":
                    Settings.Default.DropBoxAccessToken = value;
                    break;

                default:
                    Logger.Instance.Error(new KeyNotFoundException($"{tokenName} not found."));
                    break;
            }

            Settings.Default.Save();
        }
    }
}
