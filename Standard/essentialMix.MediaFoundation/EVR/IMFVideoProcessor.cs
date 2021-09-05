using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("6AB0000C-FECE-4d1f-A2AC-A9573530656E")]
	public interface IMFVideoProcessor
	{
		[PreserveSig]
		int GetAvailableVideoProcessorModes(out int lpdwNumProcessingModes,
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
			out Guid[] ppVideoProcessingModes);

		[PreserveSig]
		int GetVideoProcessorCaps([In][MarshalAs(UnmanagedType.LPStruct)] Guid lpVideoProcessorMode,
			out DXVA2VideoProcessorCaps lpVideoProcessorCaps);

		[PreserveSig]
		int GetVideoProcessorMode(out Guid lpMode);

		[PreserveSig]
		int SetVideoProcessorMode([In][MarshalAs(UnmanagedType.LPStruct)] Guid lpMode);

		[PreserveSig]
		int GetProcAmpRange(DXVA2ProcAmp dwProperty,
			out DXVA2ValueRange pPropRange);

		[PreserveSig]
		int GetProcAmpValues(DXVA2ProcAmp dwFlags,
			[Out][MarshalAs(UnmanagedType.LPStruct)]
			DXVA2ProcAmpValues Values);

		[PreserveSig]
		int SetProcAmpValues(DXVA2ProcAmp dwFlags,
			[In] DXVA2ProcAmpValues pValues);

		[PreserveSig]
		int GetFilteringRange(DXVA2Filters dwProperty,
			out DXVA2ValueRange pPropRange);

		[PreserveSig]
		int GetFilteringValue(DXVA2Filters dwProperty,
			out int pValue);

		[PreserveSig]
		int SetFilteringValue(DXVA2Filters dwProperty,
			[In] ref int pValue);

		[PreserveSig]
		int GetBackgroundColor(out int lpClrBkg);

		[PreserveSig]
		int SetBackgroundColor(int ClrBkg);
	}
}