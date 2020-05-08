using System;

namespace asm.Data
{
	[Flags]
	public enum DataViewType
	{
		None = 0,
		List = 1,
		Item = 1 << 1,
		Create = 1 << 2,
		Update = 1 << 3,
		Delete = 1 << 4,
		Details = 1 << 5,
		Custom = 1 << 6
	}
}