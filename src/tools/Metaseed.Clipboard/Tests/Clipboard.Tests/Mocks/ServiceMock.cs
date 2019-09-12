using Clipboard.Shared.Services;

namespace Clipboard.Tests.Mocks
{
    class ServiceMock : IService
    {
        public bool Reseted = true;

        public void Initialize(IServiceSettingProvider settingProvider)
        {
            Reseted = false;
        }

        public void Reset()
        {
            Reseted = true;
        }
    }
}
