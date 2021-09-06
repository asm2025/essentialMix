using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("e2cd3a4b-af25-4d3d-9110-da0e6f8ee877")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSourceBuffer
	{
		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool GetUpdating();

		[PreserveSig]
		int GetBuffered(
			out IMFMediaTimeRange ppBuffered
		);

		[PreserveSig]
		double GetTimeStampOffset();

		[PreserveSig]
		int SetTimeStampOffset(
			double offset
		);

		[PreserveSig]
		double GetAppendWindowStart();

		[PreserveSig]
		int SetAppendWindowStart(
			double time
		);

		[PreserveSig]
		double GetAppendWindowEnd();

		[PreserveSig]
		int SetAppendWindowEnd(
			double time
		);

		[PreserveSig]
		int Append(
			IntPtr pData,
			int len
		);

		[PreserveSig]
		int AppendByteStream(
			IMFByteStream pStream,
			long pMaxLen
		);

		[PreserveSig]
		int Abort();

		[PreserveSig]
		int Remove(
			double start,
			double end
		);
	}
}