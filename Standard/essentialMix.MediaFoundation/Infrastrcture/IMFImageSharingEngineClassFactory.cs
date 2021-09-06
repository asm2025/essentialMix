using System.Runtime.InteropServices;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[System.Security.SuppressUnmanagedCodeSecurity]
	[Guid("1FC55727-A7FB-4FC8-83AE-8AF024990AF1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFImageSharingEngineClassFactory
	{
		[PreserveSig]
		int CreateInstanceFromUDN(
			[MarshalAs(UnmanagedType.BStr)]
			string pUniqueDeviceName,
			out IMFImageSharingEngine ppEngine
		);
	}
}