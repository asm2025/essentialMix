using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("a7901327-05dd-4469-a7b7-0e01979e361d")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaSourceExtensionNotify
	{
		[PreserveSig]
		void OnSourceOpen();

		[PreserveSig]
		void OnSourceEnded();

		[PreserveSig]
		void OnSourceClose();
	}
}