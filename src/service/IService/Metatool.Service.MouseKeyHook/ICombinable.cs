using System.Collections.Generic;
using System.Windows.Forms;

namespace Metatool.Service;

public interface ICombinable
{
	ICombination With(Keys chordKey);
	ICombination With(IEnumerable<Keys> keys);
	ICombination Control();
	ICombination Shift();
	ICombination Alt();
}