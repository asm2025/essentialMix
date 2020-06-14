using System.Runtime.InteropServices;

namespace asm.Other.Microsoft.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct _LARGE_INTEGER
	{
		public long QuadPart;
	}
}