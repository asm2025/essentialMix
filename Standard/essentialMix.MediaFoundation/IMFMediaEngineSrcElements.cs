using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("7a5e5354-b114-4c72-b991-3131d75032ea")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineSrcElements
	{
		[PreserveSig]
		int GetLength();

		[PreserveSig]
		int GetURL(
			int index,
			[MarshalAs(UnmanagedType.BStr)] out string pURL
		);

		[PreserveSig]
		int GetType(
			int index,
			[MarshalAs(UnmanagedType.BStr)] out string pType
		);

		[PreserveSig]
		int GetMedia(
			int index,
			[MarshalAs(UnmanagedType.BStr)] out string pMedia
		);

		[PreserveSig]
		int AddElement(
			[MarshalAs(UnmanagedType.BStr)] string pURL,
			[MarshalAs(UnmanagedType.BStr)] string pType,
			[MarshalAs(UnmanagedType.BStr)] string pMedia
		);

		[PreserveSig]
		int RemoveAllElements();
	}
}