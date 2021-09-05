using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_VPDEVCAPS")]
	public struct DXVAHD_VPDEVCAPS
	{
		public DXVAHD_DEVICE_TYPE DeviceType;
		public int DeviceCaps;
		public int FeatureCaps;
		public int FilterCaps;
		public int InputFormatCaps;
		public int InputPool;
		public int OutputFormatCount;
		public int InputFormatCount;
		public int VideoProcessorCount;
		public int MaxInputStreams;
		public int MaxStreamStates;
	}
}