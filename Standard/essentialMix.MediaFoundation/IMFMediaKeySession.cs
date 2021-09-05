using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("24fa67d5-d1d0-4dc5-995c-c0efdc191fb5")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaKeySession
	{
		[PreserveSig]
		int GetError(
			out short code,
			out int systemCode);

		[PreserveSig]
		int get_KeySystem(
			[MarshalAs(UnmanagedType.BStr)] out string keySystem
		);

		[PreserveSig]
		int get_SessionId(
			[MarshalAs(UnmanagedType.BStr)] out string sessionId
		);

		[PreserveSig]
		int Update(
			IntPtr key,
			int cb
		);

		[PreserveSig]
		int Close();
	}
}