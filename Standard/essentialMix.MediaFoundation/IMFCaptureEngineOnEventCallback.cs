using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("aeda51c0-9025-4983-9012-de597b88b089")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFCaptureEngineOnEventCallback
	{
		[PreserveSig]
		int OnEvent(
			IMFMediaEvent pEvent
		);
	}
}