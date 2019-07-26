using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace Metaseed.Core
{
    public static class AutoStartManager
    {
        static readonly RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        static readonly string name      = Assembly.GetEntryAssembly().GetName().Name;
        static readonly string path      = $"{AppDomain.CurrentDomain.BaseDirectory}{name}.exe";

        public static bool IsAutoStart
        {
            get => registryKey.GetValue(name) != null;
            set
            {
                if (value) registryKey.SetValue(name, path);
                else registryKey.DeleteValue(name);
            }
        }

    }
}