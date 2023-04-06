using System.Collections.Generic;

namespace Metatool.Service;

public interface IKeyPath : IEnumerable<ICombination>
{
	bool   Disabled { get; set; }
	object Context  { get; set; }
}