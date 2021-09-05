using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("87e47623-2ceb-45d6-9b88-d8520c4dcbbc")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSourceBufferNotify
	{
		[PreserveSig]
		void OnUpdateStart();

		[PreserveSig]
		void OnAbort();

		[PreserveSig]
		void OnError(int hr);

		[PreserveSig]
		void OnUpdate();

		[PreserveSig]
		void OnUpdateEnd();
	}
}