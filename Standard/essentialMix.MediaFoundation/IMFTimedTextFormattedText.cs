using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("e13af3c1-4d47-4354-b1f5-e83ae0ecae60")]
	public interface IMFTimedTextFormattedText
	{
		[PreserveSig]
		int GetText(
			[MarshalAs(UnmanagedType.LPWStr)] out string text
		);

		[PreserveSig]
		int GetSubformattingCount();

		[PreserveSig]
		int GetSubformatting(
			int index,
			out int firstChar,
			out int charLength,
			out IMFTimedTextStyle style
		);
	}
}