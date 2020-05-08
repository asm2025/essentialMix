using System.ComponentModel.DataAnnotations;

namespace asm.Patterns.Images
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