using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("6a0083f9-8947-4c1d-9ce0-cdee22b23135")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaKeySessionNotify
	{
		[PreserveSig]
		void KeyMessage(
			[MarshalAs(UnmanagedType.BStr)] string destinationURL,
			IntPtr message,
			int cb
		);

		[PreserveSig]
		void KeyAdded();

		[PreserveSig]
		void KeyError(
			short code,
			int systemCode
		);
	}
}