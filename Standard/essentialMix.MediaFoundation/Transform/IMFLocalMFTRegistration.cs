using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Transform
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("149c4d73-b4be-4f8d-8b87-079e926b6add")]
	public interface IMFLocalMFTRegistration
	{
		[PreserveSig]
		int RegisterMFTs([In][MarshalAs(UnmanagedType.LPArray)] MFT_REGISTRATION_INFO[] pMFTs,
			int cMFTs);
	}
}