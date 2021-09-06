using System;
using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.EVR;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("77346cfd-5b49-4d73-ace0-5b52a859f2e0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFCapturePreviewSink : IMFCaptureSink
	{
		#region IMFCaptureSink Methods

		[PreserveSig]
		new int GetOutputMediaType(
			int dwSinkStreamIndex,
			out IMFMediaType ppMediaType
		);

		[PreserveSig]
		new int GetService(
			int dwSinkStreamIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid rguidService,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppUnknown
		);

		[PreserveSig]
		new int AddStream(
			int dwSourceStreamIndex,
			IMFMediaType pMediaType,
			IMFAttributes pAttributes,
			out int pdwSinkStreamIndex
		);

		[PreserveSig]
		new int Prepare();

		[PreserveSig]
		new int RemoveAllStreams();

		#endregion

		[PreserveSig]
		int SetRenderHandle(
			IntPtr handle
		);

		[PreserveSig]
		int SetRenderSurface(
			[MarshalAs(UnmanagedType.IUnknown)] object pSurface
		);

		[PreserveSig]
		int UpdateVideo(
			[In] MFVideoNormalizedRect pSrc,
			[In] MFRect pDst,
			[In] MFInt pBorderClr
		);

		[PreserveSig]
		int SetSampleCallback(
			int dwStreamSinkIndex,
			IMFCaptureEngineOnSampleCallback pCallback
		);

		[PreserveSig]
		int GetMirrorState(
			[MarshalAs(UnmanagedType.Bool)] out bool pfMirrorState
		);

		[PreserveSig]
		int SetMirrorState(
			[MarshalAs(UnmanagedType.Bool)] bool fMirrorState
		);

		[PreserveSig]
		int GetRotation(
			int dwStreamIndex,
			out int pdwRotationValue
		);

		[PreserveSig]
		int SetRotation(
			int dwStreamIndex,
			int dwRotationValue
		);

		[PreserveSig]
		int SetCustomSink(
			IMFMediaSink pMediaSink
		);
	}
}