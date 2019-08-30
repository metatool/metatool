using System;
using System.Collections.Generic;
using System.Text;
using Metaseed.MetaPlugin;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metaing
{
    public interface IMy
    {
        void SomeMethod();
    }
    public class MyClass:IMy
    {
        private readonly ILogger _logger;

        public MyClass(ILogger<MyClass> logger)
        {
            _logger = logger;
        }

        public void SomeMethod()
        {
            _logger.LogInformation("Hello");
        }
    }
}
