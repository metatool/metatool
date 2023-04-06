using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.UI;

public class Description
{
	public string Pre  { get; set; }
	public string Bold { get; set; }
	public string Post { get; set; }
}

public class TipItem
{
	public string Key { get; set; }

	Description _description;

	public Description Description => _description;

	public string DescriptionInfo
	{
		set
		{
			_description = new Description();
			if (string.IsNullOrEmpty(value)) return;
			var b                         = value.IndexOf('&');
			if (b == -1) _description.Pre = value;
			var parts                     = value.Split('&');
			_description.Pre = parts[0];
			if (parts.Length <= 1) return;
			_description.Bold = parts[1].Substring(0, 1);
			if (parts[1].Length <= 1) return;
			_description.Post = parts[1].Substring(1);
		}
	}

	public Action Action { get; set; }
}