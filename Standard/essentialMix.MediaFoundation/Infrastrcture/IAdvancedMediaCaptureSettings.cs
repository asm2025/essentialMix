using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("24E0485F-A33E-4aa1-B564-6019B1D14F65")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAdvancedMediaCaptureSettings
	{
		[PreserveSig]
		int GetDirectxDeviceManager(
			out IMFDXGIDeviceManager value
		);

		[PreserveSig]
		int SetDirectCompositionVisual(
			[MarshalAs(UnmanagedType.IUnknown)] object value
		);

		[PreserveSig]
		int UpdateVideo(
			[In] MFFloat pSrcNormalizedTop,
			[In] MFFloat pSrcNormalizedBottom,
			[In] MFFloat pSrcNormalizedRight,
			[In] MFFloat pSrcNormalizedLeft,
			[In] MFRect pDstSize,
			[In] MFInt pBorderClr
		);
	}
}