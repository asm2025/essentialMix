using System;
using essentialMix.Helpers;

namespace essentialMix.Patterns.Imaging;

public class ImageBuilder(string baseUri, string imageName) : IImageBuilder
{
	/// <inheritdoc />
	public ImageBuilder() 
		: this("/images/", null)
	{
	}

	/// <inheritdoc />
	public ImageBuilder(string baseUri) 
		: this(baseUri, null)
	{
	}

	/// <inheritdoc />
	public string BaseUri { get; set; } = baseUri;

	/// <inheritdoc />
	public string ImageName { get; set; } = imageName;

	/// <inheritdoc />
	public ImageSize ImageSize { get; set; }

	/// <inheritdoc />
	public Uri Build() { return Build(ImageName, ImageSize); }

	/// <inheritdoc />
	public Uri Build(string imageName) { return Build(imageName, ImageSize); }

	/// <inheritdoc />
	public Uri Build(ImageSize imageSize) { return Build(ImageName, imageSize); }

	/// <inheritdoc />
	public virtual Uri Build(string imageName, ImageSize imageSize)
	{
		return UriHelper.Combine(BaseUri, imageName);
	}
}