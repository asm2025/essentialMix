using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_VPCAPS")]
	public struct DXVAHD_VPCAPS
	{
		public Guid VPGuid;
		public int PastFrames;
		public int FutureFrames;
		public int ProcessorCaps;
		public int ITelecineCaps;
		public int CustomRateCount;
	}
}