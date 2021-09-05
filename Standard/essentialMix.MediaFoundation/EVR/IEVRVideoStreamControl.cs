using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("d0cfe38b-93e7-4772-8957-0400c49a4485")]
	public interface IEVRVideoStreamControl
	{
		[PreserveSig]
		int SetStreamActiveState([MarshalAs(UnmanagedType.Bool)] bool fActive);

		[PreserveSig]
		int GetStreamActiveState([MarshalAs(UnmanagedType.Bool)] out bool lpfActive);
	}
}