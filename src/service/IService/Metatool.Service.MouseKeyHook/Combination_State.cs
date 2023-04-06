using System.Collections;
using System.Collections.Generic;

namespace Metatool.Service;

public partial class Combination
{
	public bool   Disabled { get; set; }
	public object Context  { get; set; }
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<ICombination> GetEnumerator()
	{
		yield return this;
	}
}