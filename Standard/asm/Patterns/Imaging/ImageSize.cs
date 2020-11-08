using System.ComponentModel.DataAnnotations;

namespace asm.Patterns.Imaging
{
	public enum ImageSize
	{
		Default,
		Thumbnail,
		Small,
		Medium,
		Large,
		[Display(Name = "Extra Large")]
		ExtraLarge
	}
}