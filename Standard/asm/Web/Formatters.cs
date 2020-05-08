using System;

namespace asm.Web
{
	[Flags]
	public enum Formatters
	{
		None = 0,
		Xml = 1,
		Json = 1 << 1
	}
}
