using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("83A4CE40-7710-494b-A893-A472049AF630")]
	public interface IEVRTrustedVideoPlugin
	{
		[PreserveSig]
		int IsInTrustedVideoMode([MarshalAs(UnmanagedType.Bool)] out bool pYes);

		[PreserveSig]
		int CanConstrict([MarshalAs(UnmanagedType.Bool)] out bool pYes);

		[PreserveSig]
		int SetConstriction(int dwKPix);

		[PreserveSig]
		int DisableImageExport([MarshalAs(UnmanagedType.Bool)] bool bDisable);
	}
}