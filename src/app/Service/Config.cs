using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Service;
using Microsoft.Extensions.Options;

namespace Metaseed.Metatool.Service
{
    internal class ToolConfig<T>: IConfig<T>
    {
        private readonly IOptionsMonitor<T> _optionsAccessor;

        public ToolConfig()
        {

            _optionsAccessor = Services.Get<IOptionsMonitor<T>>();
        }

        public T CurrentValue => _optionsAccessor.CurrentValue;
        public IDisposable OnChange(Action<T, string> listener) => _optionsAccessor.OnChange(listener);
    }
}
