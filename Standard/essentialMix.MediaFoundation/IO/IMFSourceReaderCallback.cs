using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("deec8d99-fa1d-4d82-84c2-2c8969944867")]
	public interface IMFSourceReaderCallback
	{
		[PreserveSig]
		int OnReadSample(
			int hrStatus,
			int dwStreamIndex,
			MF_SOURCE_READER_FLAG dwStreamFlags,
			long llTimestamp,
			IMFSample pSample
		);

		[PreserveSig]
		int OnFlush(
			int dwStreamIndex
		);

		[PreserveSig]
		int OnEvent(
			int dwStreamIndex,
			IMFMediaEvent pEvent
		);
	}
}