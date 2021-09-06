using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("12558295-E399-11D5-BC2A-00B0D0F3F4AB")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFASFSplitter
	{
		[PreserveSig]
		int Initialize(
			[In] IMFASFContentInfo pIContentInfo);

		[PreserveSig]
		int SetFlags(
			[In] MFASFSplitterFlags dwFlags);

		[PreserveSig]
		int GetFlags(
			out MFASFSplitterFlags pdwFlags);

		[PreserveSig]
		int SelectStreams([In][MarshalAs(UnmanagedType.LPArray)] short[] pwStreamNumbers,
			[In] short wNumStreams);

		[PreserveSig]
		int GetSelectedStreams([Out][MarshalAs(UnmanagedType.LPArray)] short[] pwStreamNumbers,
			ref short pwNumStreams);

		[PreserveSig]
		int ParseData(
			[In] IMFMediaBuffer pIBuffer,
			[In] int cbBufferOffset,
			[In] int cbLength);

		[PreserveSig]
		int GetNextSample(
			out ASFStatusFlags pdwStatusFlags,
			out short pwStreamNumber,
			out IMFSample ppISample);

		[PreserveSig]
		int Flush();

		[PreserveSig]
		int GetLastSendTime(
			out int pdwLastSendTime);
	}
}