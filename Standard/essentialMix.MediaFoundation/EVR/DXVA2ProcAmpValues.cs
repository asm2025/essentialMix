using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVA2_ProcAmpValues")]
	public class DXVA2ProcAmpValues
	{
		public int Brightness;
		public int Contrast;
		public int Hue;
		public int Saturation;
	}
}