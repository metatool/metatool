using System;
using System.ComponentModel.Composition;
using WinReg = Microsoft.Win32.Registry;

namespace Metatool.MetaKey
{
    public interface IRegistry : IService
    {
        T GetValue<T>(string registryKeyPath, string value = null, T defaultValue = default(T));
        string GetValue(string registryKeyPath, string value = null, string defaultValue = null);
    }

    [Export(typeof(IRegistry))]
    public class Registry : IRegistry
    {
        private Registry() { }

        public static Registry Instance = new Registry();
        void IDisposable.Dispose()
        {
        }

        public T GetValue<T>(string registryKeyPath, string value = null, T defaultValue = default(T))
        {
            return (T)WinReg.GetValue(registryKeyPath, value, defaultValue);
        }

        public string GetValue(string registryKeyPath, string value = null, string defaultValue = null)
        {
            return GetValue<string>(registryKeyPath, value, defaultValue);
        }
    }
}
