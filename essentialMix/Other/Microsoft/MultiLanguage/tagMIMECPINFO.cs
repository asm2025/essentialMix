using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct tagMIMECPINFO
{
	public uint dwFlags;
	public uint uiCodePage;
	public uint uiFamilyCodePage;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40)] public ushort[] wszDescription;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)] public ushort[] wszWebCharset;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)] public ushort[] wszHeaderCharset;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)] public ushort[] wszBodyCharset;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)] public ushort[] wszFixedWidthFont;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)] public ushort[] wszProportionalFont;

	public byte bGDICharset;
}