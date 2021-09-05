using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("3137f1cd-fe5e-4805-a5d8-fb477448cb3d")]
	public interface IMFSinkWriter
	{
		[PreserveSig]
		int AddStream(
			IMFMediaType pTargetMediaType,
			out int pdwStreamIndex
		);

		[PreserveSig]
		int SetInputMediaType(
			int dwStreamIndex,
			IMFMediaType pInputMediaType,
			IMFAttributes pEncodingParameters
		);

		[PreserveSig]
		int BeginWriting();

		[PreserveSig]
		int WriteSample(
			int dwStreamIndex,
			IMFSample pSample
		);

		[PreserveSig]
		int SendStreamTick(
			int dwStreamIndex,
			long llTimestamp
		);

		[PreserveSig]
		int PlaceMarker(
			int dwStreamIndex,
			IntPtr pvContext
		);

		[PreserveSig]
		int NotifyEndOfSegment(
			int dwStreamIndex
		);

		[PreserveSig]
		int Flush(
			int dwStreamIndex
		);

		[PreserveSig]
		int Finalize_();

		[PreserveSig]
		int GetServiceForStream(
			int dwStreamIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidService,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
		);

		[PreserveSig]
		int GetStatistics(
			int dwStreamIndex,
			out MF_SINK_WRITER_STATISTICS pStats
		);
	}
}