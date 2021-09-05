using System.Runtime.InteropServices;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[System.Security.SuppressUnmanagedCodeSecurity]
	[Guid("607574EB-F4B6-45C1-B08C-CB715122901D")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPlayToControl
	{
		[PreserveSig]
		int Connect(
			IMFSharingEngineClassFactory pFactory
		);

		[PreserveSig]
		int Disconnect();
	}
}