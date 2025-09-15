using System;

namespace Metatool.Service;

[Flags]
public enum KeyEventType
{
	None  = 0,
	All   = -1,

	Down  = 1,
	Up    = 2,
	Hit   = 0xF,
	AllUp = 0x1F
}