using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("df6b87b6-ce12-45db-aba7-432fe054e57d")]
	public interface IMFTimedTextNotify
	{
		void TrackAdded(
			int trackId
		);

		void TrackRemoved(
			int trackId
		);

		void TrackSelected(
			int trackId,
			[MarshalAs(UnmanagedType.Bool)] bool selected
		);

		void TrackReadyStateChanged(
			int trackId
		);

		void Error(
			MF_TIMED_TEXT_ERROR_CODE errorCode,
			int extendedErrorCode,
			int sourceTrackId
		);

		void Cue(
			MF_TIMED_TEXT_CUE_EVENT cueEvent,
			double currentTime,
			IMFTimedTextCue cue
		);

		void Reset();
	}
}