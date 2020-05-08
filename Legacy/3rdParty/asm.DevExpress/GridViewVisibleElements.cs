using System;

namespace asm.DevExpress
{
	[Flags]
	public enum GridViewVisibleElements
	{
		None = 0x0,
		Select = 0x1,
		New = 0x2,
		Edit = 0x4,
		Delete = 0x8,
		Filter = 0x10,
		ClearFilter = 0x20,
		Search = 0x30,
		ClearSearch = 0x40
	}
}