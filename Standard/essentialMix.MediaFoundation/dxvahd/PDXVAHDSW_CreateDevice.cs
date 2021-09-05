using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_CreateDevice(IDirect3DDevice9Ex pD3DDevice,
		out IntPtr phDevice);
}