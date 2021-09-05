using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_FILTER_RANGE_DATA")]
	public struct DXVAHD_FILTER_RANGE_DATA
	{
		public int Minimum;
		public int Maximum;
		public int Default;
		public float Multiplier;
	}
}