using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("7a5645d2-43bd-47fd-87b7-dcd24cc7d692")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFCdmSuspendNotify
	{
		[PreserveSig]
		int Begin();

		[PreserveSig]
		int End();
	}
}