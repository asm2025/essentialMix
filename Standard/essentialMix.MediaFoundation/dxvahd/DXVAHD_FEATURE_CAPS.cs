using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[Flags]
	[UnmanagedName("DXVAHD_FEATURE_CAPS")]
	public enum DXVAHD_FEATURE_CAPS
	{
		AlphaFill = 0x1,
		Constriction = 0x2,
		LumaKey = 0x4,
		AlphaPalette = 0x8
	}
}