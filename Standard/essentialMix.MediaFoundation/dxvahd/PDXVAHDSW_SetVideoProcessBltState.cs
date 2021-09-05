using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_SetVideoProcessBltState(IntPtr hVideoProcessor,
		DXVAHD_BLT_STATE State,
		int DataSize,
		IntPtr pData);
}