using System.Diagnostics.CodeAnalysis;

namespace asm.Media
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum Target
	{
		Default,
		vcd,
		svcd,
		dvd,
		dv,
		dv50
	}
}