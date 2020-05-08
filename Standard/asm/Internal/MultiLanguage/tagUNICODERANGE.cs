using System.Runtime.InteropServices;

namespace asm.Internal.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct tagUNICODERANGE
	{
		public ushort wcFrom;
		public ushort wcTo;
	}
}