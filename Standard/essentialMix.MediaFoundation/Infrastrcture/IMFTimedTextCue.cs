using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("1e560447-9a2b-43e1-a94c-b0aaabfbfbc9")]
	public interface IMFTimedTextCue
	{
		[PreserveSig]
		int GetId();

		[PreserveSig]
		int GetOriginalId(
			[MarshalAs(UnmanagedType.LPWStr)] out string originalId
		);

		[PreserveSig]
		MF_TIMED_TEXT_TRACK_KIND GetCueKind();

		[PreserveSig]
		double GetStartTime();

		[PreserveSig]
		double GetDuration();

		[PreserveSig]
		int GetTrackId();

		[PreserveSig]
		int GetData(
			out IMFTimedTextBinary data
		);

		[PreserveSig]
		int GetRegion(
			out IMFTimedTextRegion region
		);

		[PreserveSig]
		int GetStyle(
			out IMFTimedTextStyle style
		);

		[PreserveSig]
		int GetLineCount();

		[PreserveSig]
		int GetLine(
			int index,
			out IMFTimedTextFormattedText line
		);
	}
}