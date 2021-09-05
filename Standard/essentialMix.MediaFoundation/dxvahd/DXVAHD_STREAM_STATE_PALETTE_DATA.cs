using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_PALETTE_DATA")]
	public struct DXVAHD_STREAM_STATE_PALETTE_DATA
	{
		public int Count;
		public int[] pEntries; // D3DCOLOR
	}
}