using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHDETW_CREATEVIDEOPROCESSOR")]
	public struct DXVAHDETW_CREATEVIDEOPROCESSOR
	{
		public long pObject;
		public long pD3D9Ex;
		public Guid VPGuid;
	}
}