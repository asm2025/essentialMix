using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("814C7B20-0FDB-4eec-AF8F-F957C8F69EDC")]
	public interface IMFVideoMixerBitmap
	{
		[PreserveSig]
		int SetAlphaBitmap([In][MarshalAs(UnmanagedType.LPStruct)] MFVideoAlphaBitmap pBmpParms);

		[PreserveSig]
		int ClearAlphaBitmap();

		[PreserveSig]
		int UpdateAlphaBitmapParameters([In] MFVideoAlphaBitmapParams pBmpParms);

		[PreserveSig]
		int GetAlphaBitmapParameters([Out] MFVideoAlphaBitmapParams pBmpParms);
	}
}