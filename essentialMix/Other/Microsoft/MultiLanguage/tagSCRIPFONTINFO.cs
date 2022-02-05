using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct tagSCRIPFONTINFO
{
	public long scripts;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)] public ushort[] wszFont;
}