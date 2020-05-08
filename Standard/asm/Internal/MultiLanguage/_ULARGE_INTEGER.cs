using System.Runtime.InteropServices;

namespace asm.Internal.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct _ULARGE_INTEGER
	{
		public ulong QuadPart;
	}
}