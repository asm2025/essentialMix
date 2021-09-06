using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("24cd47f7-81d8-4785-adb2-af697a963cd2")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFBufferListNotify
	{
		[PreserveSig]
		void OnAddSourceBuffer();

		[PreserveSig]
		void OnRemoveSourceBuffer();
	}
}