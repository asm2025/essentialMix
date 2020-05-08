using System.ComponentModel.DataAnnotations;

namespace asm.Media
{
	public enum AudioSampleRateEnum
	{
		Default,
		[Display(Name = "22050 Hz", ShortName = "22050")] Hz22050 = 22050,
		[Display(Name = "44100 Hz", ShortName = "44100")] Hz44100 = 44100,
		[Display(Name = "48000 Hz", ShortName = "48000")] Hz48000 = 48000
	}
}