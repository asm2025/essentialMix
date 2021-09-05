using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("FA99388A-4383-415A-A930-DD472A8CF6F7")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFTopologyServiceLookupClient
	{
		[PreserveSig]
		int InitServicePointers(IMFTopologyServiceLookup pLookup);

		[PreserveSig]
		int ReleaseServicePointers();
	}
}