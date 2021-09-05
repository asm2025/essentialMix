using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("19f68549-ca8a-4706-a4ef-481dbc95e12c")]
	public interface IMFCapturePhotoConfirmation
	{
		[PreserveSig]
		int SetPhotoConfirmationCallback(
			IMFAsyncCallback pNotificationCallback
		);

		[PreserveSig]
		int SetPixelFormat([In][MarshalAs(UnmanagedType.LPStruct)] Guid subtype
		);

		[PreserveSig]
		int GetPixelFormat(
			out Guid subtype
		);
	}
}