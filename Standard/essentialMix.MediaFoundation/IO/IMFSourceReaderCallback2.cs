using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("CF839FE6-8C2A-4DD2-B6EA-C22D6961AF05")]
	public interface IMFSourceReaderCallback2 : IMFSourceReaderCallback
	{
		#region IMFSourceReaderCallback

		[PreserveSig]
		new int OnReadSample(
			int hrStatus,
			int dwStreamIndex,
			MF_SOURCE_READER_FLAG dwStreamFlags,
			long llTimestamp,
			IMFSample pSample
		);

		[PreserveSig]
		new int OnFlush(
			int dwStreamIndex
		);

		[PreserveSig]
		new int OnEvent(
			int dwStreamIndex,
			IMFMediaEvent pEvent
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