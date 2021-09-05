using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("2f69d622-20b5-41e9-afdf-89ced1dda04e")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineExtension
	{
		[PreserveSig]
		int CanPlayType(
			[MarshalAs(UnmanagedType.Bool)] bool AudioOnly,
			[MarshalAs(UnmanagedType.BStr)] string MimeType,
			out MF_MEDIA_ENGINE_CANPLAY pAnswer
		);

		[PreserveSig]
		int BeginCreateObject(
			[MarshalAs(UnmanagedType.BStr)] string bstrURL,
			IMFByteStream pByteStream,
			MFObjectType type,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object punkState
		);

		[PreserveSig]
		int CancelObjectCreation([In][MarshalAs(UnmanagedType.IUnknown)] object pIUnknownCancelCookie
		);

		[PreserveSig]
		int EndCreateObject(
			IMFAsyncResult pResult,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppObject
		);
	}
}