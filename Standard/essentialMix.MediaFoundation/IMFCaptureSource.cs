using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("439a42a8-0d2c-4505-be83-f79b2a05d5c4")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFCaptureSource
	{
		[PreserveSig]
		int GetCaptureDeviceSource(
			MF_CAPTURE_ENGINE_DEVICE_TYPE mfCaptureEngineDeviceType,
			out IMFMediaSource ppMediaSource
		);

		[PreserveSig]
		int GetCaptureDeviceActivate(
			MF_CAPTURE_ENGINE_DEVICE_TYPE mfCaptureEngineDeviceType,
			out IMFActivate ppActivate
		);

		[PreserveSig]
		int GetService([In][MarshalAs(UnmanagedType.LPStruct)] Guid rguidService,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppUnknown
		);

		[PreserveSig]
		int AddEffect(
			int dwSourceStreamIndex,
			[MarshalAs(UnmanagedType.IUnknown)] object pUnknown
		);

		[PreserveSig]
		int RemoveEffect(
			int dwSourceStreamIndex,
			[MarshalAs(UnmanagedType.IUnknown)] object pUnknown
		);

		[PreserveSig]
		int RemoveAllEffects(
			int dwSourceStreamIndex
		);

		[PreserveSig]
		int GetAvailableDeviceMediaType(
			int dwSourceStreamIndex,
			int dwMediaTypeIndex,
			out IMFMediaType ppMediaType
		);

		[PreserveSig]
		int SetCurrentDeviceMediaType(
			int dwSourceStreamIndex,
			IMFMediaType pMediaType
		);

		[PreserveSig]
		int GetCurrentDeviceMediaType(
			int dwSourceStreamIndex,
			out IMFMediaType ppMediaType
		);

		[PreserveSig]
		int GetDeviceStreamCount(
			out int pdwStreamCount
		);

		[PreserveSig]
		int GetDeviceStreamCategory(
			int dwSourceStreamIndex,
			out MF_CAPTURE_ENGINE_STREAM_CATEGORY pStreamCategory
		);

		[PreserveSig]
		int GetMirrorState(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.Bool)] out bool pfMirrorState
		);

		[PreserveSig]
		int SetMirrorState(
			int dwStreamIndex,
			[MarshalAs(UnmanagedType.Bool)] bool fMirrorState
		);

		[PreserveSig]
		int GetStreamIndexFromFriendlyName(
			int uifriendlyName,
			out int pdwActualStreamIndex
		);
	}
}