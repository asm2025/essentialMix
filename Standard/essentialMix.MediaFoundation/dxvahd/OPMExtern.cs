using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.dxvahd
{
	public static class OPMExtern
	{
		[SecurityCritical]
		[DllImport("Dxva2.dll", ExactSpelling = true)]
		public static extern int DXVAHD_CreateDevice(IDirect3DDevice9Ex pD3DDevice,
			DXVAHD_CONTENT_DESC pContentDesc,
			DXVAHD_DEVICE_USAGE Usage,
			PDXVAHDSW_Plugin pPlugin,
			out IDXVAHD_Device ppDevice);
	}
}