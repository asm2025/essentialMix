using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("57BDD80A-9B38-4838-B737-C58F670D7D4F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFASFMultiplexer
	{
		[PreserveSig]
		int Initialize(
			[In] IMFASFContentInfo pIContentInfo);

		[PreserveSig]
		int SetFlags(
			[In] MFASFMultiplexerFlags dwFlags);

		[PreserveSig]
		int GetFlags(
			out MFASFMultiplexerFlags pdwFlags);

		[PreserveSig]
		int ProcessSample(
			[In] short wStreamNumber,
			[In] IMFSample pISample,
			[In] long hnsTimestampAdjust);

		[PreserveSig]
		int GetNextPacket(
			out ASFStatusFlags pdwStatusFlags,
			out IMFSample ppIPacket);

		[PreserveSig]
		int Flush();

		[PreserveSig]
		int End(
			[In] IMFASFContentInfo pIContentInfo);

		[PreserveSig]
		int GetStatistics(
			[In] short wStreamNumber,
			out ASFMuxStatistics pMuxStats);

		[PreserveSig]
		int SetSyncTolerance(
			[In] int msSyncTolerance);
	}
}