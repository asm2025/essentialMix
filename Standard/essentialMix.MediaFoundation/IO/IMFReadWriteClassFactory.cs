using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("E7FE2E12-661C-40DA-92F9-4F002AB67627")]
	public interface IMFReadWriteClassFactory
	{
		[PreserveSig]
		int CreateInstanceFromURL([In][MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string pwszURL,
			IMFAttributes pAttributes,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
		);

		[PreserveSig]
		int CreateInstanceFromObject([In][MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
			[MarshalAs(UnmanagedType.IUnknown)] object punkObject,
			IMFAttributes pAttributes,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
		);
	}
}