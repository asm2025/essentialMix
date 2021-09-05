using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
 	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("e37ceed7-340f-4514-9f4d-9c2ae026100b")]
	public interface IMFCaptureEngineOnSampleCallback2 : IMFCaptureEngineOnSampleCallback
    {
        #region IMFCaptureEngineOnSampleCallback methods

        [PreserveSig]
        new int OnSample(
            IMFSample pSample
            );

        #endregion

        [PreserveSig]
        int OnSynchronizedEvent(
            IMFMediaEvent pEvent
            );
    }
}
