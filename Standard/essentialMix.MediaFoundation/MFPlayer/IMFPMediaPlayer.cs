using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.EVR;
using essentialMix.MediaFoundation.Internal;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("A714590A-58AF-430a-85BF-44F5EC838D85")]
	public interface IMFPMediaPlayer
	{
		[PreserveSig]
		int Play();

		[PreserveSig]
		int Pause();

		[PreserveSig]
		int Stop();

		[PreserveSig]
		int FrameStep();

		[PreserveSig]
		int SetPosition([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvPositionValue
		);

		[PreserveSig]
		int GetPosition([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPMediaPlayer.GetPosition", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvPositionValue
		);

		[PreserveSig]
		int GetDuration([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPMediaPlayer.GetDuration", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvPositionValue
		);

		[PreserveSig]
		int SetRate(
			float flRate
		);

		[PreserveSig]
		int GetRate(
			out float pflRate
		);

		[PreserveSig]
		int GetSupportedRates(
			[MarshalAs(UnmanagedType.Bool)] bool fForwardDirection,
			out float pflSlowestRate,
			out float pflFastestRate
		);

		[PreserveSig]
		int GetState(
			out MFP_MEDIAPLAYER_STATE peState
		);

		[PreserveSig]
		int CreateMediaItemFromURL([In][MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
			[MarshalAs(UnmanagedType.Bool)] bool fSync,
			IntPtr dwUserData,
			out IMFPMediaItem ppMediaItem
		);

		[PreserveSig]
		int CreateMediaItemFromObject(
			[MarshalAs(UnmanagedType.IUnknown)] object pIUnknownObj,
			[MarshalAs(UnmanagedType.Bool)] bool fSync,
			IntPtr dwUserData,
			out IMFPMediaItem ppMediaItem
		);

		[PreserveSig]
		int SetMediaItem(
			IMFPMediaItem pIMFPMediaItem
		);

		[PreserveSig]
		int ClearMediaItem();

		[PreserveSig]
		int GetMediaItem(
			out IMFPMediaItem ppIMFPMediaItem
		);

		[PreserveSig]
		int GetVolume(
			out float pflVolume
		);

		[PreserveSig]
		int SetVolume(
			float flVolume
		);

		[PreserveSig]
		int GetBalance(
			out float pflBalance
		);

		[PreserveSig]
		int SetBalance(
			float flBalance
		);

		[PreserveSig]
		int GetMute(
			[MarshalAs(UnmanagedType.Bool)] out bool pfMute
		);

		[PreserveSig]
		int SetMute(
			[MarshalAs(UnmanagedType.Bool)] bool fMute
		);

		[PreserveSig]
		int GetNativeVideoSize([Out][MarshalAs(UnmanagedType.LPStruct)] MFSize pszVideo,
			[Out][MarshalAs(UnmanagedType.LPStruct)]
			MFSize pszARVideo
		);

		[PreserveSig]
		int GetIdealVideoSize([Out][MarshalAs(UnmanagedType.LPStruct)] MFSize pszMin,
			[Out][MarshalAs(UnmanagedType.LPStruct)]
			MFSize pszMax
		);

		[PreserveSig]
		int SetVideoSourceRect([In][MarshalAs(UnmanagedType.LPStruct)] MFVideoNormalizedRect pnrcSource
		);

		[PreserveSig]
		int GetVideoSourceRect([Out][MarshalAs(UnmanagedType.LPStruct)] MFVideoNormalizedRect pnrcSource
		);

		[PreserveSig]
		int SetAspectRatioMode(
			MFVideoAspectRatioMode dwAspectRatioMode
		);

		[PreserveSig]
		int GetAspectRatioMode(
			out MFVideoAspectRatioMode pdwAspectRatioMode
		);

		[PreserveSig]
		int GetVideoWindow(
			out IntPtr phwndVideo
		);

		[PreserveSig]
		int UpdateVideo();

		[PreserveSig]
		int SetBorderColor(
			Color Clr
		);

		[PreserveSig]
		int GetBorderColor(
			out Color pClr
		);

		[PreserveSig]
		int InsertEffect(
			[MarshalAs(UnmanagedType.IUnknown)] object pEffect,
			[MarshalAs(UnmanagedType.Bool)] bool fOptional
		);

		[PreserveSig]
		int RemoveEffect(
			[MarshalAs(UnmanagedType.IUnknown)] object pEffect
		);

		[PreserveSig]
		int RemoveAllEffects();

		[PreserveSig]
		int Shutdown();
	}
}