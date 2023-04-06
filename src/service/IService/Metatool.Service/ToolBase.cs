using Microsoft.Extensions.Logging;

namespace Metatool.Service;

public abstract class ToolBase : CommandPackage, IPlugin
{
	private string _id;
	public  string Id => _id??= this.GetType().Assembly.GetName().Name;

	public virtual bool OnLoaded()
	{
		// no need to add tool loaded log here
		return true;
	}

	public virtual void OnUnloading()
	{
		Logger.LogInformation($"{this.GetType().Name} start unloading, make sure resources get released here");
	}
}