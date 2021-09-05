namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHD_CreateDevice(IDirect3DDevice9Ex pD3DDevice,
		DXVAHD_CONTENT_DESC pContentDesc,
		DXVAHD_DEVICE_USAGE Usage,
		PDXVAHDSW_Plugin pPlugin,
		out IDXVAHD_Device ppDevice);
}