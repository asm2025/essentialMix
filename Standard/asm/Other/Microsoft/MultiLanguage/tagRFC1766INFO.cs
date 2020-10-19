using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal struct tagRFC1766INFO
	{
		public uint lcid;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public ushort[] wszRfc1766;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)] public ushort[] wszLocaleName;
	}
}