using System.Runtime.InteropServices;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[System.Security.SuppressUnmanagedCodeSecurity]
	[Guid("2BA61F92-8305-413B-9733-FAF15F259384")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSharingEngineClassFactory
	{
		[PreserveSig]
		int CreateInstance(
			MF_MEDIA_ENGINE_CREATEFLAGS dwFlags,
			IMFAttributes pAttr,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppEngine
		);
	}
}