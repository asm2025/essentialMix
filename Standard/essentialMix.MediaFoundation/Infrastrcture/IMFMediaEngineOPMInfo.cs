using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("765763e6-6c01-4b01-bb0f-b829f60ed28c")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineOPMInfo
	{
		[PreserveSig]
		int GetOPMInfo(
			out MF_MEDIA_ENGINE_OPM_STATUS pStatus,
			[MarshalAs(UnmanagedType.Bool)] out bool pConstricted
		);
	}
}