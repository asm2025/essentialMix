using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("666f76de-33d2-41b9-a458-29ed0a972c58")]
	public interface IMFSinkWriterCallback
	{
		[PreserveSig]
		int OnFinalize(
			int hrStatus
		);

		[PreserveSig]
		int OnMarker(
			int dwStreamIndex,
			IntPtr pvContext
		);
	}
}