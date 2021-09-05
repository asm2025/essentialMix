using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("09b2455d-b834-4f01-a347-9052e21c450e")]
	public interface IMFTimedTextStyle
	{
		[PreserveSig]
		int GetName(
			[MarshalAs(UnmanagedType.LPWStr)] out string name
		);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool IsExternal();

		[PreserveSig]
		int GetFontFamily(
			[MarshalAs(UnmanagedType.LPWStr)] out string fontFamily
		);

		[PreserveSig]
		int GetFontSize(
			out double fontSize,
			out MF_TIMED_TEXT_UNIT_TYPE unitType
		);

		[PreserveSig]
		int GetColor(
			out MFARGB color
		);

		[PreserveSig]
		int GetBackgroundColor(
			out MFARGB bgColor
		);

		[PreserveSig]
		int GetShowBackgroundAlways(
			[MarshalAs(UnmanagedType.Bool)] out bool showBackgroundAlways
		);

		[PreserveSig]
		int GetFontStyle(
			out MF_TIMED_TEXT_FONT_STYLE fontStyle
		);

		[PreserveSig]
		int GetBold(
			[MarshalAs(UnmanagedType.Bool)] out bool bold
		);

		[PreserveSig]
		int GetRightToLeft(
			[MarshalAs(UnmanagedType.Bool)] out bool rightToLeft
		);

		[PreserveSig]
		int GetTextAlignment(
			out MF_TIMED_TEXT_ALIGNMENT textAlign
		);

		[PreserveSig]
		int GetTextDecoration(
			out int textDecoration
		);

		[PreserveSig]
		int GetTextOutline(
			out MFARGB color,
			out double thickness,
			out double blurRadius,
			out MF_TIMED_TEXT_UNIT_TYPE unitType
		);
	}
}