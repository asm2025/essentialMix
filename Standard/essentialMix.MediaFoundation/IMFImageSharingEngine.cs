using System.Runtime.InteropServices;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[System.Security.SuppressUnmanagedCodeSecurity]
	[Guid("CFA0AE8E-7E1C-44D2-AE68-FC4C148A6354")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFImageSharingEngine
	{
		[PreserveSig]
		int SetSource(
			[MarshalAs(UnmanagedType.IUnknown)] object pStream
		);

		[PreserveSig]
		int GetDevice(
			out DEVICE_INFO pDevice
		);

		[PreserveSig]
		int Shutdown();
	}
}