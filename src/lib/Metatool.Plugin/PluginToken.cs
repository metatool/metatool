using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Metatool.Metatool.Plugin;
using Metatool.Reactive;
using Metatool.Service;

namespace Metatool.Plugin
{
    public class PluginToken
    {
        private Version NoneVersion = new Version(0, 0, 0, 0);
        public PluginLoader Loader;
        public ObservableFileSystemWatcher Watcher;

        public Version Version
        {
            get
            {
                var version = Loader?.MainAssembly.GetName().Version;
                if (version != NoneVersion) return version;
                var path = Loader.MainAssembly.Location;
                var verDir = new Regex("(\\d+\\.)?(\\d+\\.)?(\\*|\\d+)",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var r = verDir.Match(path);
                if (r.Value == "") return NoneVersion;
                var v = Version.Parse(r.Value);
                Func<int, int> e = (int n) => n == -1 ? 0 : n;
                if (r.Success) return new Version(e(v.Major), e(v.Minor), e(v.Build), e(v.Revision));
                return null;
            }
        }

        public List<IPlugin> Tools = new List<IPlugin>();
    }
}