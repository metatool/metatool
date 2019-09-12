using Clipboard.Core.Desktop.Tests.Mocks;
using Clipboard.Shared.Logs;
using Clipboard.Shared.Services;

namespace Clipboard.Core.Desktop.Tests
{
    public static class TestUtilities
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (!_initialized)
            {
                Logger.InstanceLogSession = new LogMock();
                Logger.InstanceLogSession.SessionStarted();
                _initialized = true;

                ServiceLocator.SettingProvider = new ServiceSettingProviderMock();
            }
            else
            {
                Logger.InstanceLogSession.SessionStopped();
            }

            GetSettingProvider().ResetSettings();
            ServiceLocator.ResetAll();
        }

        internal static ServiceSettingProviderMock GetSettingProvider()
        {
            if (ServiceLocator.SettingProvider == null || !(ServiceLocator.SettingProvider is ServiceSettingProviderMock))
            {
                ServiceLocator.SettingProvider = new ServiceSettingProviderMock();
            }

            return (ServiceSettingProviderMock)ServiceLocator.SettingProvider;
        }
    }
}
