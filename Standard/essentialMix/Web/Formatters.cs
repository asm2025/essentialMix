using System;

namespace essentialMix.Web
{
	[Flags]
	public enum Formatters
	{
		None = 0,
		Xml = 1,
		Json = 1 << 1
	}
}
