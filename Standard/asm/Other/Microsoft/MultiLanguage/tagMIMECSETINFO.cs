using System.Runtime.InteropServices;

namespace asm.Other.Microsoft.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct tagMIMECSETINFO
	{
		public uint uiCodePage;
		public uint uiInternetEncoding;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)] public ushort[] wszCharset;
	}
}