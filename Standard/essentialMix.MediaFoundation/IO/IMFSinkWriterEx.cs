using System;
using System.Runtime.InteropServices;
using System.Security;
using essentialMix.MediaFoundation.Transform;

namespace essentialMix.MediaFoundation.IO
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("588d72ab-5Bc1-496a-8714-b70617141b25")]
	public interface IMFSinkWriterEx : IMFSinkWriter
	{
		#region IMFSinkWriter methods

		[PreserveSig]
		new int AddStream(
			IMFMediaType pTargetMediaType,
			out int pdwStreamIndex
		);

		[PreserveSig]
		new int SetInputMediaType(
			int dwStreamIndex,
			IMFMediaType pInputMediaType,
			IMFAttributes pEncodingParameters
		);

		[PreserveSig]
		new int BeginWriting();

		[PreserveSig]
		new int WriteSample(
			int dwStreamIndex,
			IMFSample pSample
		);

		[PreserveSig]
		new int SendStreamTick(
			int dwStreamIndex,
			long llTimestamp
		);

		[PreserveSig]
		new int PlaceMarker(
			int dwStreamIndex,
			IntPtr pvContext
		);

		[PreserveSig]
		new int NotifyEndOfSegment(
			int dwStreamIndex
		);

		[PreserveSig]
		new int Flush(
			int dwStreamIndex
		);

		[PreserveSig]
		new int Finalize_();

		[PreserveSig]
		new int GetServiceForStream(
			int dwStreamIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidService,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
		);

		[PreserveSig]
		new int GetStatistics(
			int dwStreamIndex,
			out MF_SINK_WRITER_STATISTICS pStats
		);

		#endregion

		[PreserveSig]
		int GetTransformForStream(
			int dwStreamIndex,
			int dwTransformIndex,
			out Guid pGuidCategory,
			out IMFTransform ppTransform);
	}
}