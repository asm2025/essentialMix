using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct tagSCRIPTINFO
{
	public byte ScriptId;
	public uint uiCodePage;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x30)] public ushort[] wszDescription;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)] public ushort[] wszFixedWidthFont;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)] public ushort[] wszProportionalFont;
}