using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("23ff334c-442c-445f-bccc-edc438aa11e2")]
	public interface IMFTimedTextTrackList
	{
		[PreserveSig]
		int GetLength();

		[PreserveSig]
		int GetTrack(
			int index,
			out IMFTimedTextTrack track
		);

		[PreserveSig]
		int GetTrackById(
			int trackId,
			out IMFTimedTextTrack track
		);
	}
}