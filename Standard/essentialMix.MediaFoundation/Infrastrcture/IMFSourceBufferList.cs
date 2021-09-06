using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("249981f8-8325-41f3-b80c-3b9e3aad0cbe")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSourceBufferList
	{
		[PreserveSig]
		int GetLength();

		[PreserveSig]
		IMFSourceBuffer GetSourceBuffer(
			int index
		);
	}
}