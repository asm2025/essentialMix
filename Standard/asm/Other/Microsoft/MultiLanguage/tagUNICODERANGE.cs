using System.Runtime.InteropServices;

namespace asm.Other.Microsoft.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct tagUNICODERANGE
	{
		public ushort wcFrom;
		public ushort wcTo;
	}
}