using System.ComponentModel.DataAnnotations;

namespace essentialMix.Patterns.Imaging
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