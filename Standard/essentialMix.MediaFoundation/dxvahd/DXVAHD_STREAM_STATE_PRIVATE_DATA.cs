using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_PRIVATE_DATA")]
	public struct DXVAHD_STREAM_STATE_PRIVATE_DATA
	{
		public Guid Guid;
		public int DataSize;
		public IntPtr pData;
	}
}