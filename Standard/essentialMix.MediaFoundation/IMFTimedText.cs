using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("1f2a94c9-a3df-430d-9d0f-acd85ddc29af")]
	public interface IMFTimedText
	{
		[PreserveSig]
		int RegisterNotifications(
			IMFTimedTextNotify notify
		);

		[PreserveSig]
		int SelectTrack(
			int trackId,
			[MarshalAs(UnmanagedType.Bool)] bool selected
		);

		[PreserveSig]
		int AddDataSource(
			IMFByteStream byteStream,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string label,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string language,
			MF_TIMED_TEXT_TRACK_KIND kind,
			[MarshalAs(UnmanagedType.Bool)] bool isDefault,
			out int trackId
		);

		[PreserveSig]
		int AddDataSourceFromUrl([In][MarshalAs(UnmanagedType.LPWStr)] string url,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string label,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string language,
			MF_TIMED_TEXT_TRACK_KIND kind,
			[MarshalAs(UnmanagedType.Bool)] bool isDefault,
			out int trackId
		);

		[PreserveSig]
		int AddTrack([In][MarshalAs(UnmanagedType.LPWStr)] string label,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string language,
			MF_TIMED_TEXT_TRACK_KIND kind,
			out IMFTimedTextTrack track
		);

		[PreserveSig]
		int RemoveTrack(
			IMFTimedTextTrack track
		);

		[PreserveSig]
		int GetCueTimeOffset(
			out double offset
		);

		[PreserveSig]
		int SetCueTimeOffset(
			double offset
		);

		[PreserveSig]
		int GetTracks(
			out IMFTimedTextTrackList tracks
		);

		[PreserveSig]
		int GetActiveTracks(
			out IMFTimedTextTrackList activeTracks
		);

		[PreserveSig]
		int GetTextTracks(
			out IMFTimedTextTrackList textTracks
		);

		[PreserveSig]
		int GetMetadataTracks(
			out IMFTimedTextTrackList metadataTracks
		);

		[PreserveSig]
		int SetInBandEnabled(
			[MarshalAs(UnmanagedType.Bool)] bool enabled
		);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool IsInBandEnabled();
	}
}