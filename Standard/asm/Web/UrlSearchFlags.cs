using System;

namespace asm.Web
{
	[Flags]
	public enum UrlSearchFlags
	{
		None = 0,
		Title = 1,
		Buffer = 1 << 1,
		IgnoreCase = 1 << 2,
		TitleAndBuffer = Title | Buffer
	}
}