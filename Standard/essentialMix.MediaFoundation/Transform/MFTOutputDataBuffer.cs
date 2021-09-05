using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFT_OUTPUT_DATA_BUFFER")]
	public struct MFTOutputDataBuffer
	{
		public int dwStreamID;
		public IntPtr pSample; // Doesn't release correctly when marshaled as IMFSample
		public MFTOutputDataBufferFlags dwStatus;

		[MarshalAs(UnmanagedType.Interface)]
		public IMFCollection pEvents;
	}
}