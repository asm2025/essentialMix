using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.Internal;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("766C8FFB-5FDB-4fea-A28D-B912996F51BD")]
	public interface IMFPMediaPlayerCallback
	{
		[PreserveSig]
		int OnMediaPlayerEvent(
			[In][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPMediaPlayerCallback.OnMediaPlayerEvent", MarshalTypeRef = typeof(EHMarshaler))] MFP_EVENT_HEADER pEventHeader
		);
	}
}