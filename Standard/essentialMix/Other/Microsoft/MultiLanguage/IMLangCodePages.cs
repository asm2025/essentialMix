using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[ComImport]
[Guid("359F3443-BD4A-11D0-B188-00AA0038C969")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMLangCodePages
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void GetCharCodePages([In] ushort chSrc, out uint pdwCodePages);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void GetStrCodePages([In] ref ushort pszSrc,
		[In] int cchSrc,
		[In] uint dwPriorityCodePages,
		out uint pdwCodePages,
		out int pcchCodePages);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void CodePageToCodePages([In] uint uCodePage, out uint pdwCodePages);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void CodePagesToCodePage([In] uint dwCodePages, [In] uint uDefaultCodePage, out uint puCodePage);
}