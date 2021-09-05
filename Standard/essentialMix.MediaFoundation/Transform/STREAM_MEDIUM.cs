using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("STREAM_MEDIUM")]
	public struct STREAM_MEDIUM
	{
		private readonly Guid gidMedium;
		private readonly int unMediumInstance;
	}
}