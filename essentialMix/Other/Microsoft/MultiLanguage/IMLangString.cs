using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Other.Microsoft.MultiLanguage;

[ComImport]
[Guid("C04D65CE-B70D-11D0-B188-00AA0038C969")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMLangString
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Sync([In] int fNoAccess);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	int GetLength();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void SetMLStr([In] int lDestPos,
		[In] int lDestLen,
		[In] [MarshalAs(UnmanagedType.IUnknown)] object pSrcMLStr,
		[In] int lSrcPos,
		[In] int lSrcLen);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void GetMLStr([In] int lSrcPos,
		[In] int lSrcLen,
		[In] [MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
		[In] uint dwClsContext,
		[In] ref Guid piid,
		[MarshalAs(UnmanagedType.IUnknown)] out object ppDestMLStr,
		out int plDestPos,
		out int plDestLen);
}