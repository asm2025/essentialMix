using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_D3DFORMAT_DATA")]
	public struct DXVAHD_STREAM_STATE_D3DFORMAT_DATA
	{
		public int Format; // D3DFORMAT
	}
}