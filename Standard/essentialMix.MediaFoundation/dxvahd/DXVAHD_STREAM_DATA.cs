using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_DATA")]
	public struct DXVAHD_STREAM_DATA
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool Enable;

		public int OutputIndex;
		public int InputFrameOrField;
		public int PastFrames;
		public int FutureFrames;
		public IDirect3DSurface9[] ppPastSurfaces;
		public IDirect3DSurface9 pInputSurface;
		public IDirect3DSurface9[] ppFutureSurfaces;
	}
}