using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_ProposeVideoPrivateFormat(IntPtr hDevice,
		ref int pFormat // D3DFORMAT
	);
}