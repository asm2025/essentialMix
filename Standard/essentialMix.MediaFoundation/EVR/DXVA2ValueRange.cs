using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVA2_ValueRange")]
	public struct DXVA2ValueRange
	{
		public int MinValue;
		public int MaxValue;
		public int DefaultValue;
		public int StepSize;
	}
}