using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("4ae3a412-0545-43c4-bf6f-6b97a5c6c432")]
	public interface IMFTimedTextBinary
	{
		[PreserveSig]
		int GetData(
			out IntPtr data,
			out int length
		);
	}
}