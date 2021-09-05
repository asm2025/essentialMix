using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFVideoAlphaBitmap")]
	public class MFVideoAlphaBitmap
	{
		public bool GetBitmapFromDC;
		public IntPtr stru;
		public MFVideoAlphaBitmapParams paras;
	}
}