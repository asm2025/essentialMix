using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVA2_VideoProcessorCaps")]
	public struct DXVA2VideoProcessorCaps
	{
		public int DeviceCaps;
		public int InputPool;
		public int NumForwardRefSamples;
		public int NumBackwardRefSamples;
		public int Reserved;
		public int DeinterlaceTechnology;
		public int ProcAmpControlCaps;
		public int VideoProcessorOperations;
		public int NoiseFilterTechnology;
		public int DetailFilterTechnology;
	}
}
