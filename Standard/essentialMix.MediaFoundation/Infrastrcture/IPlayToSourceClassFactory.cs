using System.Runtime.InteropServices;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[System.Security.SuppressUnmanagedCodeSecurity]
	[Guid("842B32A3-9B9B-4D1C-B3F3-49193248A554")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPlayToSourceClassFactory
	{
		[PreserveSig]
		int CreateInstance(
			PLAYTO_SOURCE_CREATEFLAGS dwFlags,
			IPlayToControl pControl,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppSource
		);
	}
}