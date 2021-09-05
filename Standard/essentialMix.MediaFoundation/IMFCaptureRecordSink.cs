using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("3323b55a-f92a-4fe2-8edc-e9bfc0634d77")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFCaptureRecordSink : IMFCaptureSink
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
		int SetOutputByteStream(
			IMFByteStream pByteStream,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidContainerType
		);

		[PreserveSig]
		int SetOutputFileName(
			[MarshalAs(UnmanagedType.LPWStr)] string fileName
		);

		[PreserveSig]
		int SetSampleCallback(
			int dwStreamSinkIndex,
			IMFCaptureEngineOnSampleCallback pCallback
		);

		[PreserveSig]
		int SetCustomSink(
			IMFMediaSink pMediaSink
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
	}
}