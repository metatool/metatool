using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace Metatool.Plugin
{
    public abstract class PluginBase : IPlugin
    {
        private readonly ILogger _logger;

        protected PluginBase(ILogger logger)
        {
            _logger                  =  logger;
        }

        protected ILogger Log => _logger;

        private void OnPluginUnloadingRequested(AssemblyLoadContext obj)
        {
            this.OnUnloading();
        }

        public virtual bool OnLoaded()
        {
            _logger.LogInformation($"{this.GetType().Name} loaded.");
            return true;
        }

        public virtual void OnUnloading()
        {
            _logger.LogInformation($"{this.GetType().Name} start unloading, make sure resources get released here");
        }
    }

}
