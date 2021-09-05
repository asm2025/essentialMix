using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHDETW_DESTROYVIDEOPROCESSOR")]
	public struct DXVAHDETW_DESTROYVIDEOPROCESSOR
	{
		public long pObject;
	}
}