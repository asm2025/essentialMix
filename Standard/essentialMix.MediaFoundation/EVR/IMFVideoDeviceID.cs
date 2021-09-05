using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("A38D9567-5A9C-4F3C-B293-8EB415B279BA")]
	public interface IMFVideoDeviceID
	{
		[PreserveSig]
		int GetDeviceID(out Guid pDeviceID);
	}
}