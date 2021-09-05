using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("83E91E85-82C1-4ea7-801D-85DC50B75086")]
	public interface IEVRFilterConfig
	{
		[PreserveSig]
		int SetNumberOfStreams(int dwMaxStreams);

		[PreserveSig]
		int GetNumberOfStreams(out int pdwMaxStreams);
	}
}