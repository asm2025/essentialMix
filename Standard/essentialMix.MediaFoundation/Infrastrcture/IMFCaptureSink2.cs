using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("f9e4219e-6197-4b5e-b888-bee310ab2c59")]
	public interface IMFCaptureSink2 : IMFCaptureSink
	{
		#region IMFCaptureEngineOnSampleCallback methods

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
		int SetOutputMediaType(
			int dwStreamIndex,
			IMFMediaType pMediaType,
			IMFAttributes pEncodingAttributes
		);
	}
}