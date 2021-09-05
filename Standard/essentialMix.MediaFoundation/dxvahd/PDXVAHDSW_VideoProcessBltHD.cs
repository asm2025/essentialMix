using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_VideoProcessBltHD(IntPtr hVideoProcessor,
		IDirect3DSurface9 pOutputSurface,
		int OutputFrame,
		int StreamCount,
		DXVAHD_STREAM_DATA[] pStreams);
}