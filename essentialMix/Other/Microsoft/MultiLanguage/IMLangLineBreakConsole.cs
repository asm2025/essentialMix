using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("F5BE2EE1-BFD7-11D0-B188-00AA0038C969")]
internal interface IMLangLineBreakConsole
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void BreakLineML([In] [MarshalAs(UnmanagedType.Interface)] CMLangString pSrcMLStr,
		[In] int lSrcPos,
		[In] int lSrcLen,
		[In] int cMinColumns,
		[In] int cMaxColumns,
		out int plLineLen,
		out int plSkipLen);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void BreakLineW([In] uint locale,
		[In] ref ushort pszSrc,
		[In] int cchSrc,
		[In] int cMaxColumns,
		out int pcchLine,
		out int pcchSkip);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void BreakLineA([In] uint locale,
		[In] uint uCodePage,
		[In] ref sbyte pszSrc,
		[In] int cchSrc,
		[In] int cMaxColumns,
		out int pcchLine,
		out int pcchSkip);
}