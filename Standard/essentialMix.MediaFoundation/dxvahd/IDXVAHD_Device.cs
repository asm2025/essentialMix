using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.dxvahd
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("95f12dfd-d77e-49be-815f-57d579634d6d")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXVAHD_Device
	{
		[PreserveSig]
		int CreateVideoSurface(int Width,
			int Height,
			int Format, // D3DFORMAT
			int Pool, // D3DPOOL
			int Usage,
			DXVAHD_SURFACE_TYPE Type,
			int NumSurfaces,
			out IDirect3DSurface9[] ppSurfaces,
			ref IntPtr pSharedHandle);

		[PreserveSig]
		int GetVideoProcessorDeviceCaps(out DXVAHD_VPDEVCAPS pCaps);

		[PreserveSig]
		int GetVideoProcessorOutputFormats(int Count,
			out int[] pFormats // D3DFORMAT
		);

		[PreserveSig]
		int GetVideoProcessorInputFormats(int Count,
			out int[] pFormats // D3DFORMAT
		);

		[PreserveSig]
		int GetVideoProcessorCaps(int Count,
			out DXVAHD_VPCAPS[] pCaps);

		[PreserveSig]
		int GetVideoProcessorCustomRates([In][MarshalAs(UnmanagedType.LPStruct)] Guid pVPGuid,
			int Count,
			out DXVAHD_CUSTOM_RATE_DATA[] pRates);

		[PreserveSig]
		int GetVideoProcessorFilterRange(DXVAHD_FILTER Filter,
			out DXVAHD_FILTER_RANGE_DATA pRange);

		[PreserveSig]
		int CreateVideoProcessor([In][MarshalAs(UnmanagedType.LPStruct)] Guid pVPGuid,
			out IDXVAHD_VideoProcessor ppVideoProcessor);
	}
}