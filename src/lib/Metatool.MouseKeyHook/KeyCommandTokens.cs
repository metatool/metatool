using System.Collections.Generic;
using System.Linq;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public class KeyCommandTokens : List<IKeyCommand>, IKeyCommand
{
	public string Id
	{
		get => this.Aggregate("", (a, c) => a + c.Id);
		set
		{
			for (var i = 0; i < this.Count; i++)
			{
				var k = this[i];
				k.Id = value;
			}
		}
	}

	public void Remove()
	{
		this.ForEach(t => t.Remove());
		this.Clear();
	}

	public bool Change(ICommandTrigger<IKeyEventArgs> trigger)
	{
		this.ForEach(t => t.Change(trigger));
		return true;
	}

	public bool Change(IHotkey key)
	{
		this.ForEach(t => t.Change(key));
		return true;
	}

	internal IMetaKey metaKey => new MetaKeys(this.Cast<KeyCommandToken>().Select(t => t.metaKey).ToList());
}