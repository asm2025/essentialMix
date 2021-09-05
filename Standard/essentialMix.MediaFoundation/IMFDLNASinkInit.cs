using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("0C012799-1B61-4C10-BDA9-04445BE5F561")]
	public interface IMFDLNASinkInit
	{
		[PreserveSig]
		int Initialize(
			IMFByteStream pByteStream,
			[MarshalAs(UnmanagedType.Bool)] bool fPal
		);
	}
}