using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("8822c32d-654e-4233-bf21-d7f2e67d30d4")]
	public interface IMFTimedTextTrack
	{
		[PreserveSig]
		int GetId();

		[PreserveSig]
		int GetLabel(
			[MarshalAs(UnmanagedType.LPWStr)] out string label
		);

		[PreserveSig]
		int SetLabel([In][MarshalAs(UnmanagedType.LPWStr)] string label
		);

		[PreserveSig]
		int GetLanguage(
			[MarshalAs(UnmanagedType.LPWStr)] out string language
		);

		[PreserveSig]
		MF_TIMED_TEXT_TRACK_KIND GetTrackKind();

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool IsInBand();

		[PreserveSig]
		int GetInBandMetadataTrackDispatchType(
			[MarshalAs(UnmanagedType.LPWStr)] out string dispatchType
		);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool IsActive();

		[PreserveSig]
		MF_TIMED_TEXT_ERROR_CODE GetErrorCode();

		[PreserveSig]
		int GetExtendedErrorCode();

		[PreserveSig]
		int GetDataFormat(out Guid format);

		[PreserveSig]
		MF_TIMED_TEXT_TRACK_READY_STATE GetReadyState();

		[PreserveSig]
		int GetCueList(
			out IMFTimedTextCueList cues
		);
	}
}