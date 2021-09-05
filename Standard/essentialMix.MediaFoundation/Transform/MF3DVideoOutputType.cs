using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[Flags]
	[UnmanagedName("MF3DVideoOutputType")]
	public enum MF3DVideoOutputType
	{
		BaseView = 0,
		Stereo = 1
	}
}