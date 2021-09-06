using System.Runtime.InteropServices;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[System.Security.SuppressUnmanagedCodeSecurity]
	[Guid("524D2BC4-B2B1-4FE5-8FAC-FA4E4512B4E0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaSharingEngineClassFactory
	{
		[PreserveSig]
		int CreateInstance(
			int dwFlags,
			IMFAttributes pAttr,
			out IMFMediaSharingEngine ppEngine
		);
	}
}