using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("fee7c112-e776-42b5-9bbf-0048524e2bd5")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineNotify
	{
		[PreserveSig]
		int EventNotify(
			MF_MEDIA_ENGINE_EVENT eventid,
			IntPtr param1,
			int param2
		);
	}
}