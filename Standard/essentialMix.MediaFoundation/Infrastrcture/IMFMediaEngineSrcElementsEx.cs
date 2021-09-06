using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("654a6bb3-e1a3-424a-9908-53a43a0dfda0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineSrcElementsEx : IMFMediaEngineSrcElements
	{
		#region IMFMediaEngineSrcElements methods

		[PreserveSig]
		new int GetLength();

		[PreserveSig]
		new int GetURL(
			int index,
			[MarshalAs(UnmanagedType.BStr)] out string pURL
		);

		[PreserveSig]
		new int GetType(
			int index,
			[MarshalAs(UnmanagedType.BStr)] out string pType
		);

		[PreserveSig]
		new int GetMedia(
			int index,
			[MarshalAs(UnmanagedType.BStr)] out string pMedia
		);

		[PreserveSig]
		new int AddElement(
			[MarshalAs(UnmanagedType.BStr)] string pURL,
			[MarshalAs(UnmanagedType.BStr)] string pType,
			[MarshalAs(UnmanagedType.BStr)] string pMedia
		);

		[PreserveSig]
		new int RemoveAllElements();

		#endregion

		[PreserveSig]
		int AddElementEx(
			[MarshalAs(UnmanagedType.BStr)] string pURL,
			[MarshalAs(UnmanagedType.BStr)] string pType,
			[MarshalAs(UnmanagedType.BStr)] string pMedia,
			[MarshalAs(UnmanagedType.BStr)] string keySystem
		);

		[PreserveSig]
		int GetKeySystem(
			int index,
			[MarshalAs(UnmanagedType.BStr)] out string pType
		);
	}
}