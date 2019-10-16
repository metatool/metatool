using Microsoft.Extensions.Logging;

namespace Metatool.Plugin
{
    public abstract class PluginBase : IPlugin
    {
        private readonly ILogger _logger;

        protected PluginBase()
        {
            var loggerType = typeof(ILogger<>).MakeGenericType(this.GetType());
            _logger = Services.Get(loggerType) as ILogger;
        }

        protected ILogger Logger => _logger;

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