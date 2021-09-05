using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("ad128745-211b-40a0-9981-fe65f166d0fd")]
	public interface IMFTimedTextCueList
	{
		[PreserveSig]
		int GetLength();

		[PreserveSig]
		int GetCueByIndex(
			int index,
			out IMFTimedTextCue cue
		);

		[PreserveSig]
		int GetCueById(
			int id,
			out IMFTimedTextCue cue
		);

		[PreserveSig]
		int GetCueByOriginalId([In][MarshalAs(UnmanagedType.LPWStr)] string originalId,
			out IMFTimedTextCue cue
		);

		[PreserveSig]
		int AddTextCue(
			double start,
			double duration,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string text,
			out IMFTimedTextCue cue
		);

		[PreserveSig]
		int AddDataCue(
			double start,
			double duration,
			[MarshalAs(UnmanagedType.LPArray)] byte[] data,
			int dataSize,
			out IMFTimedTextCue cue
		);

		[PreserveSig]
		int RemoveCue(
			IMFTimedTextCue cue
		);
	}
}