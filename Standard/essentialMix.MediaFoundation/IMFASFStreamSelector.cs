using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("d01bad4a-4fa0-4a60-9349-c27e62da9d41")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFASFStreamSelector
	{
		[PreserveSig]
		int GetStreamCount(
			out int pcStreams);

		[PreserveSig]
		int GetOutputCount(
			out int pcOutputs);

		[PreserveSig]
		int GetOutputStreamCount(
			[In] int dwOutputNum,
			out int pcStreams);

		[PreserveSig]
		int GetOutputStreamNumbers(
			[In] int dwOutputNum,
			[Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I2)]
			short[] rgwStreamNumbers);

		[PreserveSig]
		int GetOutputFromStream(
			[In] short wStreamNum,
			out int pdwOutput);

		[PreserveSig]
		int GetOutputOverride(
			[In] int dwOutputNum,
			out ASFSelectionStatus pSelection);

		[PreserveSig]
		int SetOutputOverride(
			[In] int dwOutputNum,
			[In] ASFSelectionStatus Selection);

		[PreserveSig]
		int GetOutputMutexCount(
			[In] int dwOutputNum,
			out int pcMutexes);

		[PreserveSig]
		int GetOutputMutex(
			[In] int dwOutputNum,
			[In] int dwMutexNum,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppMutex);

		[PreserveSig]
		int SetOutputMutexSelection(
			[In] int dwOutputNum,
			[In] int dwMutexNum,
			[In] short wSelectedRecord);

		[PreserveSig]
		int GetBandwidthStepCount(
			out int pcStepCount);

		[PreserveSig]
		int GetBandwidthStep(
			[In] int dwStepNum,
			out int pdwBitrate,
			[Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I2)]
			short[] rgwStreamNumbers,
			[Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)]
			ASFSelectionStatus[] rgSelections);

		[PreserveSig]
		int BitrateToStepNumber(
			[In] int dwBitrate,
			out int pdwStepNum);

		[PreserveSig]
		int SetStreamSelectorFlags(
			[In] MFASFStreamSelectorFlags dwStreamSelectorFlags);
	}
}