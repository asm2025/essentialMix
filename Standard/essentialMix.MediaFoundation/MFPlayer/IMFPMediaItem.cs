using System;
using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.Internal;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("90EB3E6B-ECBF-45cc-B1DA-C6FE3EA70D57")]
	public interface IMFPMediaItem
	{
		[PreserveSig]
		int GetMediaPlayer(
			out IMFPMediaPlayer ppMediaPlayer
		);

		[PreserveSig]
		int GetURL(
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszURL
		);

		[PreserveSig]
		int GetObject(
			[MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknown
		);

		[PreserveSig]
		int GetUserData(
			out IntPtr pdwUserData
		);

		[PreserveSig]
		int SetUserData(
			IntPtr dwUserData
		);

		[PreserveSig]
		int GetStartStopPosition([Out][MarshalAs(UnmanagedType.LPStruct)] MFGuid pguidStartPositionType,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPMediaItem.GetStartStopPosition.0", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvStartValue,
			[Out][MarshalAs(UnmanagedType.LPStruct)]
			MFGuid pguidStopPositionType,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPMediaItem.GetStartStopPosition.1", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvStopValue
		);

		[PreserveSig]
		int SetStartStopPosition([In][MarshalAs(UnmanagedType.LPStruct)] MFGuid pguidStartPositionType,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvStartValue,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			MFGuid pguidStopPositionType,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvStopValue
		);

		[PreserveSig]
		int HasVideo(
			[MarshalAs(UnmanagedType.Bool)] out bool pfHasVideo,
			[MarshalAs(UnmanagedType.Bool)] out bool pfSelected
		);

		[PreserveSig]
		int HasAudio(
			[MarshalAs(UnmanagedType.Bool)] out bool pfHasAudio,
			[MarshalAs(UnmanagedType.Bool)] out bool pfSelected
		);

		[PreserveSig]
		int IsProtected(
			[MarshalAs(UnmanagedType.Bool)] out bool pfProtected
		);

		[PreserveSig]
		int GetDuration([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidPositionType,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPMediaItem.GetDuration", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvDurationValue
		);

		[PreserveSig]
		int GetNumberOfStreams(
			out int pdwStreamCount
		);

		[PreserveSig]
		int GetStreamSelection(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.Bool)] out bool pfEnabled
		);

		[PreserveSig]
		int SetStreamSelection(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.Bool)] bool fEnabled
		);

		[PreserveSig]
		int GetStreamAttribute(
			int dwStreamIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidMFAttribute,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPMediaItem.GetStreamAttribute", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvValue
		);

		[PreserveSig]
		int GetPresentationAttribute([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidMFAttribute,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPMediaItem.GetPresentationAttribute", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvValue
		);

		[PreserveSig]
		int GetCharacteristics(
			out MFP_MEDIAITEM_CHARACTERISTICS pCharacteristics
		);

		[PreserveSig]
		int SetStreamSink(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.IUnknown)] object pMediaSink
		);

		[PreserveSig]
		int GetMetadata(
			out IPropertyStore ppMetadataStore
		);
	}
}