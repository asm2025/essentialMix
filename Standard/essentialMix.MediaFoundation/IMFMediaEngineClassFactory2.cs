using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("09083cef-867f-4bf6-8776-dee3a7b42fca")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineClassFactory2
	{
		[PreserveSig]
		int CreateMediaKeys2(
			[MarshalAs(UnmanagedType.BStr)] string keySystem,
			[MarshalAs(UnmanagedType.BStr)] string defaultCdmStorePath,
			[MarshalAs(UnmanagedType.BStr)] string inprivateCdmStorePath,
			out IMFMediaKeys ppKeys
		);
	}
}