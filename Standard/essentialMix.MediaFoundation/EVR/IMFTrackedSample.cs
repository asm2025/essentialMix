using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("245BF8E9-0755-40F7-88A5-AE0F18D55E17")]
	public interface IMFTrackedSample
	{
		[PreserveSig]
		int SetAllocator([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pSampleAllocator,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnkState);
	}
}