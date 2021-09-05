using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Transform
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("BF94C121-5B05-4E6F-8000-BA598961414D")]
	public interface IMFTransform
	{
		[PreserveSig]
		int GetStreamLimits([Out] MFInt pdwInputMinimum,
			[Out] MFInt pdwInputMaximum,
			[Out] MFInt pdwOutputMinimum,
			[Out] MFInt pdwOutputMaximum);

		[PreserveSig]
		int GetStreamCount([Out] MFInt pcInputStreams,
			[Out] MFInt pcOutputStreams);

		[PreserveSig]
		int GetStreamIDs(int dwInputIDArraySize,
			[Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
			int[] pdwInputIDs,
			int dwOutputIDArraySize,
			[Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			int[] pdwOutputIDs);

		[PreserveSig]
		int GetInputStreamInfo(int dwInputStreamID,
			out MFTInputStreamInfo pStreamInfo);

		[PreserveSig]
		int GetOutputStreamInfo(int dwOutputStreamID,
			out MFTOutputStreamInfo pStreamInfo);

		[PreserveSig]
		int GetAttributes([MarshalAs(UnmanagedType.Interface)] out IMFAttributes pAttributes);

		[PreserveSig]
		int GetInputStreamAttributes(int dwInputStreamID,
			[MarshalAs(UnmanagedType.Interface)]
			out IMFAttributes pAttributes);

		[PreserveSig]
		int GetOutputStreamAttributes(int dwOutputStreamID,
			[MarshalAs(UnmanagedType.Interface)]
			out IMFAttributes pAttributes);

		[PreserveSig]
		int DeleteInputStream(int dwStreamID);

		[PreserveSig]
		int AddInputStreams(int cStreams,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
			int[] adwStreamIDs);

		[PreserveSig]
		int GetInputAvailableType(int dwInputStreamID,
			int dwTypeIndex,
			[MarshalAs(UnmanagedType.Interface)]
			out IMFMediaType ppType);

		[PreserveSig]
		int GetOutputAvailableType(int dwOutputStreamID,
			int dwTypeIndex,
			[MarshalAs(UnmanagedType.Interface)]
			out IMFMediaType ppType);

		[PreserveSig]
		int SetInputType(int dwInputStreamID,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFMediaType pType,
			MFTSetTypeFlags dwFlags);

		[PreserveSig]
		int SetOutputType(int dwOutputStreamID,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFMediaType pType,
			MFTSetTypeFlags dwFlags);

		[PreserveSig]
		int GetInputCurrentType(int dwInputStreamID,
			[MarshalAs(UnmanagedType.Interface)]
			out IMFMediaType ppType);

		[PreserveSig]
		int GetOutputCurrentType(int dwOutputStreamID,
			[MarshalAs(UnmanagedType.Interface)]
			out IMFMediaType ppType);

		[PreserveSig]
		int GetInputStatus(int dwInputStreamID,
			out MFTInputStatusFlags pdwFlags);

		[PreserveSig]
		int GetOutputStatus(out MFTOutputStatusFlags pdwFlags);

		[PreserveSig]
		int SetOutputBounds(long hnsLowerBound,
			long hnsUpperBound);

		[PreserveSig]
		int ProcessEvent(int dwInputStreamID,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFMediaEvent pEvent);

		[PreserveSig]
		int ProcessMessage(MFTMessageType eMessage,
			IntPtr ulParam);

		[PreserveSig]
		int ProcessInput(int dwInputStreamID,
			[MarshalAs(UnmanagedType.Interface)]
			IMFSample pSample,
			int dwFlags // Must be zero
		);

		[PreserveSig]
		int ProcessOutput(MFTProcessOutputFlags dwFlags,
			int cOutputBufferCount,
			[In][Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
			MFTOutputDataBuffer[] pOutputSamples,
			out ProcessOutputStatus pdwStatus);
	}
}