using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHDETW_VIDEOPROCESSBLTHD")]
	public struct DXVAHDETW_VIDEOPROCESSBLTHD
	{
		public long pObject;
		public long pOutputSurface;
		public MFRect TargetRect;
		public int OutputFormat; // D3DFORMAT
		public int ColorSpace;
		public int OutputFrame;
		public int StreamCount;

		[MarshalAs(UnmanagedType.Bool)]
		public bool Enter;
	}
}