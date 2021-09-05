using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("50dc93e4-ba4f-4275-ae66-83e836e57469")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineEME
	{
		[PreserveSig]
		int get_Keys(
			out IMFMediaKeys keys // check null
		);

		[PreserveSig]
		int SetMediaKeys(
			IMFMediaKeys keys
		);
	}
}