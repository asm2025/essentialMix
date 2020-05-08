using System.Runtime.InteropServices;

namespace asm.Internal.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct _RemotableHandle
	{
		public int fContext;
		public __MIDL_IWinTypes_0009 u;
	}
}