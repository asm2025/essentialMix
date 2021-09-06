using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("c8d22afc-bc47-4bdf-9b04-787e49ce3f58")]
	public interface IMFTimedTextRegion
	{
		[PreserveSig]
		int GetName(
			[MarshalAs(UnmanagedType.LPWStr)] out string name
		);

		[PreserveSig]
		int GetPosition(
			out double pX,
			out double pY,
			out MF_TIMED_TEXT_UNIT_TYPE unitType
		);

		[PreserveSig]
		int GetExtent(
			out double pWidth,
			out double pHeight,
			out MF_TIMED_TEXT_UNIT_TYPE unitType
		);

		[PreserveSig]
		int GetBackgroundColor(
			out MFARGB bgColor
		);

		[PreserveSig]
		int GetWritingMode(
			out MF_TIMED_TEXT_WRITING_MODE writingMode
		);

		[PreserveSig]
		int GetDisplayAlignment(
			out MF_TIMED_TEXT_DISPLAY_ALIGNMENT displayAlign
		);

		[PreserveSig]
		int GetLineHeight(
			out double pLineHeight,
			out MF_TIMED_TEXT_UNIT_TYPE unitType
		);

		[PreserveSig]
		int GetClipOverflow(
			[MarshalAs(UnmanagedType.Bool)] out bool clipOverflow
		);

		[PreserveSig]
		int GetPadding(
			out double before,
			out double start,
			out double after,
			out double end,
			out MF_TIMED_TEXT_UNIT_TYPE unitType
		);

		[PreserveSig]
		int GetWrap(
			[MarshalAs(UnmanagedType.Bool)] out bool wrap
		);

		[PreserveSig]
		int GetZIndex(
			out int zIndex
		);

		[PreserveSig]
		int GetScrollMode(
			out MF_TIMED_TEXT_SCROLL_MODE scrollMode
		);
	}
}