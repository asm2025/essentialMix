using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_SetVideoProcessStreamState(IntPtr hVideoProcessor,
		int StreamNumber,
		DXVAHD_STREAM_STATE State,
		int DataSize,
		IntPtr pData);
}