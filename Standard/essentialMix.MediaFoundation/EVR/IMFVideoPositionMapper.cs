using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("1F6A9F17-E70B-4E24-8AE4-0B2C3BA7A4AE")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFVideoPositionMapper
	{
		[PreserveSig]
		int MapOutputCoordinateToInputStream([In] float xOut,
			[In] float yOut,
			[In] int dwOutputStreamIndex,
			[In] int dwInputStreamIndex,
			out float pxIn,
			out float pyIn);
	}
}