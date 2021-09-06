using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("5cb31c05-61ff-418f-afda-caaf41421a38")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaKeys
	{
		[PreserveSig]
		int CreateSession(
			[MarshalAs(UnmanagedType.BStr)] string mimeType,
			IntPtr initData,
			int cb,
			IntPtr customData,
			int cbCustomData,
			IMFMediaKeySessionNotify notify,
			out IMFMediaKeySession ppSession
		);

		[PreserveSig]
		int get_KeySystem(
			[MarshalAs(UnmanagedType.BStr)] out string keySystem
		);

		[PreserveSig]
		int Shutdown();

		[PreserveSig]
		int GetSuspendNotify(
			out IMFCdmSuspendNotify notify
		);
	}
}