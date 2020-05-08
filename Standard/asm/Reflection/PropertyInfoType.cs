using System;

namespace asm.Reflection
{
	[Flags]
	public enum PropertyInfoType
	{
		Default = 0,
		Get = 1,
		Set = 1 << 1,
		All = Get | Set
	}
}