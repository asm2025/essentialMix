using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("2456BD58-C067-4513-84FE-8D0C88FFDC61")]
	public interface IMFSinkWriterCallback2 : IMFSinkWriterCallback
	{

		#region IMFSinkWriterCallback

		[PreserveSig]
		new int OnFinalize(
			int hrStatus
		);

		[PreserveSig]
		new int OnMarker(
			int dwStreamIndex,
			IntPtr pvContext
		);

		#endregion

		[PreserveSig]
		int OnTransformChange();

		[PreserveSig]
		int OnStreamError(
			int dwStreamIndex,
			int hrStatus);

	}
}