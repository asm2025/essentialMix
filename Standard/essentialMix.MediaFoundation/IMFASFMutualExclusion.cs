using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("12558291-E399-11D5-BC2A-00B0D0F3F4AB")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFASFMutualExclusion
	{
		[PreserveSig]
		int GetType(
			out Guid pguidType);

		[PreserveSig]
		int SetType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidType);

		[PreserveSig]
		int GetRecordCount(
			out int pdwRecordCount);

		[PreserveSig]
		int GetStreamsForRecord(
			[In] int dwRecordNumber,
			[In][Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)]
			short [] pwStreamNumArray,
			ref int pcStreams);

		[PreserveSig]
		int AddStreamForRecord(
			[In] int dwRecordNumber,
			[In] short wStreamNumber);

		[PreserveSig]
		int RemoveStreamFromRecord(
			[In] int dwRecordNumber,
			[In] short wStreamNumber);

		[PreserveSig]
		int RemoveRecord(
			[In] int dwRecordNumber);

		[PreserveSig]
		int AddRecord(
			out int pdwRecordNumber);

		[PreserveSig]
		int Clone(
			out IMFASFMutualExclusion ppIMutex);
	}
}