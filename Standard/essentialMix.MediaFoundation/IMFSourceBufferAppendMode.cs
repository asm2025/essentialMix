using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("19666fb4-babe-4c55-bc03-0a074da37e2a")]
	public interface IMFSourceBufferAppendMode
	{
		[PreserveSig]
		MF_MSE_APPEND_MODE GetAppendMode();

		[PreserveSig]
		int SetAppendMode(
			MF_MSE_APPEND_MODE mode
		);
	}
}