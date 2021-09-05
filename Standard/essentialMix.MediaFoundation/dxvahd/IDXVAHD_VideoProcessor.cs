using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.dxvahd
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("95f4edf4-6e03-4cd7-be1b-3075d665aa52")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXVAHD_VideoProcessor
	{
		[PreserveSig]
		int SetVideoProcessBltState(DXVAHD_BLT_STATE State,
			int DataSize,
			IntPtr pData);

		[PreserveSig]
		int GetVideoProcessBltState(DXVAHD_BLT_STATE State,
			int DataSize,
			IntPtr pData);

		[PreserveSig]
		int SetVideoProcessStreamState(int StreamNumber,
			DXVAHD_STREAM_STATE State,
			int DataSize,
			IntPtr pData);

		[PreserveSig]
		int GetVideoProcessStreamState(int StreamNumber,
			DXVAHD_STREAM_STATE State,
			int DataSize,
			IntPtr pData);

		[PreserveSig]
		int VideoProcessBltHD(IDirect3DSurface9 pOutputSurface,
			int OutputFrame,
			int StreamCount,
			DXVAHD_STREAM_DATA[] pStreams);
	}
}