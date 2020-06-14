using System.Runtime.InteropServices;

namespace asm.Other.Microsoft.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct _FILETIME
	{
		public uint dwLowDateTime;
		public uint dwHighDateTime;
	}
}