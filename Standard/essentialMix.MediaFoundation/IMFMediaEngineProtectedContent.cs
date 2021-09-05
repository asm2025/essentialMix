using System;
using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.EVR;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("9f8021e8-9c8c-487e-bb5c-79aa4779938c")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineProtectedContent
	{
		[PreserveSig]
		int ShareResources([In][MarshalAs(UnmanagedType.IUnknown)] object pUnkDeviceContext
		);

		[PreserveSig]
		int GetRequiredProtections(
			out MF_MEDIA_ENGINE_FRAME_PROTECTION_FLAGS pFrameProtectionFlags
		);

		[PreserveSig]
		int SetOPMWindow(
			IntPtr hwnd
		);

		[PreserveSig]
		int TransferVideoFrame([In][MarshalAs(UnmanagedType.IUnknown)] object pDstSurf,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			MFVideoNormalizedRect pSrc,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			MFRect pDst,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			MFARGB pBorderClr,
			out MF_MEDIA_ENGINE_FRAME_PROTECTION_FLAGS pFrameProtectionFlags
		);

		[PreserveSig]
		int SetContentProtectionManager(
			IMFContentProtectionManager pCPM
		);

		[PreserveSig]
		int SetApplicationCertificate(
			IntPtr pbBlob,
			int cbBlob
		);
	}
}