using System;

namespace asm.Web
{
	[Flags]
	public enum XmlIndexMatchType : short
	{
		None = 0,
		Type = 1,
		Name = 1 << 1
	}
}