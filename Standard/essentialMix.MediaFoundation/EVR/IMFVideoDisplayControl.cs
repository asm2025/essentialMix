using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("A490B1E4-AB84-4D31-A1B2-181E03B1077A")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFVideoDisplayControl
	{
		[PreserveSig]
		int GetNativeVideoSize([Out] MFSize pszVideo,
			[Out] MFSize pszARVideo);

		[PreserveSig]
		int GetIdealVideoSize([Out] MFSize pszMin,
			[Out] MFSize pszMax);

		[PreserveSig]
		int SetVideoPosition([In] MFVideoNormalizedRect pnrcSource,
			[In] MFRect prcDest);

		[PreserveSig]
		int GetVideoPosition([Out] MFVideoNormalizedRect pnrcSource,
			[Out] MFRect prcDest);

		[PreserveSig]
		int SetAspectRatioMode([In] MFVideoAspectRatioMode dwAspectRatioMode);

		[PreserveSig]
		int GetAspectRatioMode(out MFVideoAspectRatioMode pdwAspectRatioMode);

		[PreserveSig]
		int SetVideoWindow([In] IntPtr hwndVideo);

		[PreserveSig]
		int GetVideoWindow(out IntPtr phwndVideo);

		[PreserveSig]
		int RepaintVideo();

		[PreserveSig]
		int GetCurrentImage(
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFVideoDisplayControl.GetCurrentImage", MarshalTypeRef = typeof(BMMarshaler))]
			BitmapInfoHeader pBih,
			out IntPtr pDib,
			out int pcbDib,
			out long pTimeStamp);

		[PreserveSig]
		int SetBorderColor([In] int Clr);

		[PreserveSig]
		int GetBorderColor(out int pClr);

		[PreserveSig]
		int SetRenderingPrefs([In] MFVideoRenderPrefs dwRenderFlags);

		[PreserveSig]
		int GetRenderingPrefs(out MFVideoRenderPrefs pdwRenderFlags);

		[PreserveSig]
		int SetFullscreen([In][MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

		[PreserveSig]
		int GetFullscreen([MarshalAs(UnmanagedType.Bool)] out bool pfFullscreen);
	}
}