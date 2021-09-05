﻿using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_DESTINATION_RECT_DATA")]
	public struct DXVAHD_STREAM_STATE_DESTINATION_RECT_DATA
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool Enable;

		public MFRect DestinationRect;
	}
}