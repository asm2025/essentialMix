using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("a724b056-1b2e-4642-a6f3-db9420c52908")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineSupportsSourceTransfer
	{
		[PreserveSig]
		int ShouldTransferSource(
			[MarshalAs(UnmanagedType.Bool)] out bool pfShouldTransfer
		);

		[PreserveSig]
		int DetachMediaSource(
			out IMFByteStream ppByteStream,
			out IMFMediaSource ppMediaSource,
			out IMFMediaSourceExtension ppMSE
		);

		[PreserveSig]
		int AttachMediaSource(
			IMFByteStream pByteStream,
			IMFMediaSource pMediaSource,
			IMFMediaSourceExtension pMSE
		);
	}
}