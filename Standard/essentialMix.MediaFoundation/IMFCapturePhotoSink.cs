using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("d2d43cc8-48bb-4aa7-95db-10c06977e777")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFCapturePhotoSink : IMFCaptureSink
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
		int SetOutputFileName(
			[MarshalAs(UnmanagedType.LPWStr)] string fileName
		);

		[PreserveSig]
		int SetSampleCallback(
			IMFCaptureEngineOnSampleCallback pCallback
		);

		[PreserveSig]
		int SetOutputByteStream(
			IMFByteStream pByteStream
		);
	}
}