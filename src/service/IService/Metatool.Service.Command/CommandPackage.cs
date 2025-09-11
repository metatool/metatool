using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Metatool.Service;

public abstract class CommandPackage
{
	private readonly ILogger _logger;
	protected ILogger Logger => _logger;

	protected CommandPackage()
	{
		var loggerType = typeof(ILogger<>).MakeGenericType(GetType());//will get the child type and create a logger for the child
		_logger = Services.Get(loggerType) as ILogger;
	}

    /// <summary>
    /// public IKeyCommand Esc = Caps.MapOnHit(Key.Esc, e => !e.IsVirtual);
    /// </summary>
    /// <returns></returns>
    protected IEnumerable<(FieldInfo fi, ICommandToken token)> GetCommands()
	{
		var commands = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
			.Where(f => typeof(ICommandToken).IsAssignableFrom(f.FieldType))
			.Select(fi => (fi, fi.GetValue(this) as ICommandToken));
		return commands;
	}

	protected void RegisterCommands()
	{
		GetCommands().Where(k=>k.token!=null).ToList().ForEach(c =>
		{
			var (fi, metaKey) = c;
			if (string.IsNullOrEmpty(metaKey.Id))
				metaKey.Id = GetType().FullName + "." + fi.Name;
		});
	}
}