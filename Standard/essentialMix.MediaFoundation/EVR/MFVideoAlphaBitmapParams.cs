using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFVideoAlphaBitmapParams")]
	public class MFVideoAlphaBitmapParams
	{
		public MFVideoAlphaBitmapFlags dwFlags;
		public int clrSrcKey;
		public MFRect rcSrc;
		public MFVideoNormalizedRect nrcDest;
		public float fAlpha;
		public int dwFilterMode;
	}
}