using Metatool.Service.MouseKey;

namespace Metatool.Input;

public class MetaKeys(List<IMetaKey> keys) : List<IMetaKey>(keys), IMetaKey
{
    public string Id
	{
		get => this.Aggregate("", (a, c) => a + c.Id);
		set
		{
			for (var i = 0; i < Count; i++)
			{
				var k = (MetaKey) this[i];
				k.Id = value; //$"{value}_{i}-{k.KeyEvent}
			}
		}
	}

	public bool Disable
	{
		get => this.First().Disable;
		set => ForEach(k => k.Disable = value);
	}

	public IHotkey Hotkey
	{
		get => this.First().Hotkey;
		set => ForEach(k => k.Hotkey = value);
	}

	public IMetaKey ChangeHotkey(IHotkey hotkey)
	{
		Hotkey = hotkey;
		return this;
	}

	public void ChangeDescription(string description)
	{
		ForEach(k =>k.ChangeDescription(description));
	}

	public void Remove()
	{
		ForEach(k => k.Remove());
	}
}