using System.Collections.Generic;

namespace Metatool.Service.MouseKey;

public interface IKeyPath : IEnumerable<ICombination>
{
	bool   Disabled { get; set; }
	object Context  { get; set; }
	string PathString() => string.Join(" , ", this);
}